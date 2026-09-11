using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Application.DTOs;
using VitalSync.PatientService.Domain.Entities;
using VitalSync.PatientService.Domain.Repositories;

namespace VitalSync.PatientService.Application.Services
{
    public class TriageAppService : ITriageAppService
    {
        private readonly ITriageRecordRepository _triageRepository;
        private readonly IPatientRepository _patientRepository;
        public TriageAppService(ITriageRecordRepository triageRepository, IPatientRepository patientRepository)
        {
            _triageRepository = triageRepository;
            _patientRepository = patientRepository;
        }
        public async Task<TriageRecordDto?> GetTriageRecordByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var record = await _triageRepository.GetByIdAsync(id, cancellationToken);
            return record == null ? null : MapToDto(record);
        }
        public async Task<IEnumerable<TriageRecordDto>> GetTriageRecordsForPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            var records = await _triageRepository.GetByPatientIdAsync(patientId, cancellationToken);
            return records.Select(MapToDto);
        }
        public async Task<TriageRecordDto> CreateTriageAssessmentAsync(CreateTriageDto dto, CancellationToken cancellationToken = default)
        {
            // Verify Patient exists before creating triage record
            var patient = await _patientRepository.GetByIdAsync(dto.PatientId, cancellationToken);
            if (patient == null)
            {
                throw new KeyNotFoundException($"Patient with ID '{dto.PatientId}' was not found.");
            }
            var triageRecord = new TriageRecord(
                dto.PatientId,
                dto.ReportedSymptoms,
                dto.BloodPressure,
                dto.HeartRateBpm,
                dto.OxygenSaturationPercent,
                dto.Priority
            );
            await _triageRepository.AddAsync(triageRecord, cancellationToken);
            await _triageRepository.SaveChangesAsync(cancellationToken);
            return MapToDto(triageRecord);
        }
        public async Task<bool> UpdateTriageStatusAsync(Guid triageId, string newStatus, CancellationToken cancellationToken = default)
        {
            var record = await _triageRepository.GetByIdAsync(triageId, cancellationToken);
            if (record == null) return false;
            record.UpdateStatus(newStatus);
            await _triageRepository.UpdateAsync(record, cancellationToken);
            return await _triageRepository.SaveChangesAsync(cancellationToken);
        }
        private static TriageRecordDto MapToDto(TriageRecord record)
        {
            return new TriageRecordDto
            {
                Id = record.Id,
                PatientId = record.PatientId,
                ReportedSymptoms = record.ReportedSymptoms,
                BloodPressure = record.BloodPressure,
                HeartRateBpm = record.HeartRateBpm,
                OxygenSaturationPercent = record.OxygenSaturationPercent,
                Priority = record.Priority,
                Status = record.Status,
                CreatedAt = record.CreatedAt
            };
        }
    }
}
