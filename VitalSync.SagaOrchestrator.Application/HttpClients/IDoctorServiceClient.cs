using System;
using System.Threading;
using System.Threading.Tasks;
using VitalSync.SagaOrchestrator.Application.DTOs;

namespace VitalSync.SagaOrchestrator.Application.HttpClients
{
    public interface IDoctorServiceClient
    {
        Task<DoctorReservationResult> ReserveDoctorSlotAsync(Guid triageId, Guid patientId, string specialty, CancellationToken cancellationToken = default);
        Task<bool> ReleaseDoctorSlotAsync(Guid reservationId, string reason, CancellationToken cancellationToken = default);
    }

    public interface ILabServiceClient
    {
        Task<LabReservationResult> ReserveLabSlotAsync(Guid triageId, Guid patientId, string equipmentType, CancellationToken cancellationToken = default);
        Task<bool> ReleaseLabSlotAsync(Guid reservationId, string reason, CancellationToken cancellationToken = default);
    }
}
