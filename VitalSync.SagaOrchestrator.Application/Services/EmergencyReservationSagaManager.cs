using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitalSync.SagaOrchestrator.Application.DTOs;
using VitalSync.SagaOrchestrator.Application.HttpClients;
using VitalSync.SagaOrchestrator.Domain.Entities;
using VitalSync.SagaOrchestrator.Domain.Repositories;

namespace VitalSync.SagaOrchestrator.Application.Services
{
    public interface IEmergencyReservationSagaManager
    {
        Task<EmergencyReservationSagaResponseDto> ExecuteSagaAsync(EmergencyReservationSagaDto dto, CancellationToken cancellationToken = default);
        Task<SagaState?> GetSagaStateAsync(Guid sagaId, CancellationToken cancellationToken = default);
    }

    public class EmergencyReservationSagaManager : IEmergencyReservationSagaManager
    {
        private readonly ISagaRepository _sagaRepository;
        private readonly IDoctorServiceClient _doctorServiceClient;
        private readonly ILabServiceClient _labServiceClient;
        private readonly ILogger<EmergencyReservationSagaManager> _logger;

        public EmergencyReservationSagaManager(
            ISagaRepository sagaRepository,
            IDoctorServiceClient doctorServiceClient,
            ILabServiceClient labServiceClient,
            ILogger<EmergencyReservationSagaManager> logger)
        {
            _sagaRepository = sagaRepository;
            _doctorServiceClient = doctorServiceClient;
            _labServiceClient = labServiceClient;
            _logger = logger;
        }

        public async Task<EmergencyReservationSagaResponseDto> ExecuteSagaAsync(EmergencyReservationSagaDto dto, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var saga = new SagaState(dto.TriageId, dto.PatientId, dto.RequiredSpecialty, dto.RequiredEquipmentType);
            await _sagaRepository.AddAsync(saga, cancellationToken);
            await _sagaRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Starting Emergency Reservation Saga [{SagaId}]...", saga.Id);

            // STEP 1: Reserve Doctor Slot
            var doctorResult = await _doctorServiceClient.ReserveDoctorSlotAsync(dto.TriageId, dto.PatientId, dto.RequiredSpecialty, cancellationToken);
            if (!doctorResult.Success)
            {
                saga.RecordFailure("DoctorSlotReservation", doctorResult.ErrorMessage);
                await _sagaRepository.SaveChangesAsync(cancellationToken);
                stopwatch.Stop();
                return CreateResponse(saga, false, stopwatch.ElapsedMilliseconds);
            }

            saga.RecordDoctorReservation(doctorResult.ReservationId, doctorResult.DoctorId, doctorResult.DoctorName);
            await _sagaRepository.SaveChangesAsync(cancellationToken);

            // STEP 2: Reserve Lab Equipment Slot
            var labResult = await _labServiceClient.ReserveLabSlotAsync(dto.TriageId, dto.PatientId, dto.RequiredEquipmentType, cancellationToken);
            if (!labResult.Success)
            {
                _logger.LogWarning("Step 2 (Lab Reservation) failed for Saga [{SagaId}]: {Error}. Initiating Compensating Action...", saga.Id, labResult.ErrorMessage);
                saga.AddLog($"Step 2 Failed: {labResult.ErrorMessage}. Triggering compensation rollback...");

                // COMPENSATING ACTION: Release Doctor Slot
                var compStopwatch = Stopwatch.StartNew();
                bool released = await _doctorServiceClient.ReleaseDoctorSlotAsync(doctorResult.ReservationId, $"Compensating Action: Lab reservation failed - {labResult.ErrorMessage}", cancellationToken);
                compStopwatch.Stop();

                saga.RecordCompensation($"Released Doctor Slot [{doctorResult.ReservationId}] in {compStopwatch.ElapsedMilliseconds}ms (Success: {released}).");
                await _sagaRepository.SaveChangesAsync(cancellationToken);
                stopwatch.Stop();

                return CreateResponse(saga, true, stopwatch.ElapsedMilliseconds);
            }

            // STEP 3: Complete Saga
            saga.RecordLabReservation(labResult.ReservationId, labResult.EquipmentId, labResult.EquipmentName);
            await _sagaRepository.SaveChangesAsync(cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation("Emergency Reservation Saga [{SagaId}] completed successfully in {Elapsed}ms.", saga.Id, stopwatch.ElapsedMilliseconds);
            return CreateResponse(saga, false, stopwatch.ElapsedMilliseconds);
        }

        public async Task<SagaState?> GetSagaStateAsync(Guid sagaId, CancellationToken cancellationToken = default)
        {
            return await _sagaRepository.GetByIdAsync(sagaId, cancellationToken);
        }

        private static EmergencyReservationSagaResponseDto CreateResponse(SagaState saga, bool compensatingExecuted, long executionTimeMs)
        {
            return new EmergencyReservationSagaResponseDto(
                SagaId: saga.Id,
                TriageId: saga.TriageId,
                PatientId: saga.PatientId,
                Status: saga.Status.ToString(),
                DoctorReservationId: saga.DoctorReservationId,
                DoctorName: saga.DoctorName,
                LabReservationId: saga.LabReservationId,
                EquipmentName: saga.EquipmentName,
                CompensatingRollbackExecuted: compensatingExecuted,
                ExecutionTimeMs: executionTimeMs,
                ExecutionLogs: saga.ExecutionLogs
            );
        }
    }
}
