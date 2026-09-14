using System;

namespace VitalSync.SagaOrchestrator.Application.DTOs
{
    public record EmergencyReservationSagaDto(
        Guid TriageId,
        Guid PatientId,
        string RequiredSpecialty,
        string RequiredEquipmentType
    );

    public record DoctorReservationResult(
        bool Success,
        Guid ReservationId,
        Guid DoctorId,
        string DoctorName,
        string ErrorMessage
    );

    public record LabReservationResult(
        bool Success,
        Guid ReservationId,
        Guid EquipmentId,
        string EquipmentName,
        string ErrorMessage
    );

    public record EmergencyReservationSagaResponseDto(
        Guid SagaId,
        Guid TriageId,
        Guid PatientId,
        string Status,
        Guid? DoctorReservationId,
        string? DoctorName,
        Guid? LabReservationId,
        string? EquipmentName,
        bool CompensatingRollbackExecuted,
        long ExecutionTimeMs,
        System.Collections.Generic.List<string> ExecutionLogs
    );
}
