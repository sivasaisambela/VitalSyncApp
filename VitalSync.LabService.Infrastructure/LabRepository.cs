using Microsoft.EntityFrameworkCore;
using VitalSync.LabService.Domain.Entities;
using VitalSync.LabService.Domain.Repositories;
using VitalSync.LabService.Infrastructure.Persistence;

namespace VitalSync.LabService.Infrastructure.Repositories
{
    public class LabRepository : ILabRepository
    {
        private readonly LabDbContext _context;

        public LabRepository(LabDbContext context)
        {
            _context = context;
        }

        public async Task<LabEquipment?> GetAvailableEquipmentByTypeAsync(string equipmentType, CancellationToken cancellationToken = default)
        {
            return await _context.Equipment
                .Where(e => e.IsOperational && e.EquipmentType.ToLower() == equipmentType.ToLower())
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddSlotAsync(LabSlot slot, CancellationToken cancellationToken = default)
        {
            await _context.LabSlots.AddAsync(slot, cancellationToken);
        }

        public async Task<LabSlot?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default)
        {
            return await _context.LabSlots
                .Include(s => s.Equipment)
                .FirstOrDefaultAsync(s => s.Id == slotId, cancellationToken);
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
