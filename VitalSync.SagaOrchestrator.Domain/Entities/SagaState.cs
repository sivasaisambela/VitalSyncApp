using System;
using System.Collections.Generic;

namespace VitalSync.SagaOrchestrator.Domain.Entities
{
    public class SagaState
    {
        public Guid Id { get; private set; }
        public Guid TriageId { get; private set; }
        public Guid PatientId { get; private set; }
        public string RequiredSpecialty { get; private set; }
        public string RequiredEquipmentType { get; private set; }
        
        public Guid? DoctorReservationId { get; private set; }
        public Guid? DoctorId { get; private set; }
        public string? DoctorName { get; private set; }
        
        public Guid? LabReservationId { get; private set; }
        public Guid? EquipmentId { get; private set; }
        public string? EquipmentName { get; private set; }

        public SagaStatus Status { get; private set; }
        public string CurrentStep { get; private set; }
        public List<string> ExecutionLogs { get; private set; } = new();
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        private SagaState() { RequiredSpecialty = string.Empty; RequiredEquipmentType = string.Empty; CurrentStep = string.Empty; } // EF Core

        public SagaState(Guid triageId, Guid patientId, string requiredSpecialty, string requiredEquipmentType)
        {
            Id = Guid.NewGuid();
            TriageId = triageId;
            PatientId = patientId;
            RequiredSpecialty = requiredSpecialty;
            RequiredEquipmentType = requiredEquipmentType;
            Status = SagaStatus.Pending;
            CurrentStep = "SagaInitialized";
            CreatedAt = DateTime.UtcNow;
            AddLog($"Saga [{Id}] initialized for Patient [{PatientId}], Triage [{TriageId}].");
        }

        public void RecordDoctorReservation(Guid reservationId, Guid doctorId, string doctorName)
        {
            DoctorReservationId = reservationId;
            DoctorId = doctorId;
            DoctorName = doctorName;
            Status = SagaStatus.DoctorReserved;
            CurrentStep = "DoctorSlotReserved";
            AddLog($"Step 1 Success: Doctor slot [{reservationId}] reserved for Dr. {doctorName} [{doctorId}].");
        }

        public void RecordLabReservation(Guid reservationId, Guid equipmentId, string equipmentName)
        {
            LabReservationId = reservationId;
            EquipmentId = equipmentId;
            EquipmentName = equipmentName;
            Status = SagaStatus.Completed;
            CurrentStep = "SagaCompletedSuccessfully";
            CompletedAt = DateTime.UtcNow;
            AddLog($"Step 2 Success: Lab slot [{reservationId}] reserved for Equipment {equipmentName} [{equipmentId}]. Saga Completed.");
        }

        public void RecordCompensation(string reason)
        {
            Status = SagaStatus.RolledBack;
            CurrentStep = "CompensatingActionExecuted";
            CompletedAt = DateTime.UtcNow;
            AddLog($"Compensating Action Executed: Released Doctor slot [{DoctorReservationId}]. Reason: {reason}");
        }

        public void RecordFailure(string step, string error)
        {
            Status = SagaStatus.Failed;
            CurrentStep = step;
            CompletedAt = DateTime.UtcNow;
            AddLog($"Saga Failed at Step [{step}]. Error: {error}");
        }

        public void AddLog(string message)
        {
            ExecutionLogs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {message}");
        }
    }
}

