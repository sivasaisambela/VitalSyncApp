using System;
using System.Threading;
using System.Threading.Tasks;
using VitalSync.DoctorService.Domain.Entities;

namespace VitalSync.DoctorService.Domain.Repositories
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetAvailableDoctorBySpecialtyAsync(string specialty, CancellationToken cancellationToken = default);
        Task AddSlotAsync(DoctorSlot slot, CancellationToken cancellationToken = default);
        Task<DoctorSlot?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
