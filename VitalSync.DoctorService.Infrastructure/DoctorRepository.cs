using Microsoft.EntityFrameworkCore;
using VitalSync.DoctorService.Domain.Entities;
using VitalSync.DoctorService.Domain.Repositories;
using VitalSync.DoctorService.Infrastructure.Persistence;

namespace VitalSync.DoctorService.Infrastructure.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly DoctorDbContext _context;

        public DoctorRepository(DoctorDbContext context)
        {
            _context = context;
        }

        public async Task<Doctor?> GetAvailableDoctorBySpecialtyAsync(string specialty, CancellationToken cancellationToken = default)
        {
            return await _context.Doctors
                .Where(d => d.IsOnDuty && d.Specialty.ToLower() == specialty.ToLower())
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddSlotAsync(DoctorSlot slot, CancellationToken cancellationToken = default)
        {
            await _context.DoctorSlots.AddAsync(slot, cancellationToken);
        }

        public async Task<DoctorSlot?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default)
        {
            return await _context.DoctorSlots
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.Id == slotId, cancellationToken);
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
