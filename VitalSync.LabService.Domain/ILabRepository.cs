using System;
using System.Threading;
using System.Threading.Tasks;
using VitalSync.LabService.Domain.Entities;

namespace VitalSync.LabService.Domain.Repositories
{
    public interface ILabRepository
    {
        Task<LabEquipment?> GetAvailableEquipmentByTypeAsync(string equipmentType, CancellationToken cancellationToken = default);
        Task AddSlotAsync(LabSlot slot, CancellationToken cancellationToken = default);
        Task<LabSlot?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
