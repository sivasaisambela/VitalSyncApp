using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Domain.Entities;
using VitalSync.PatientService.Domain.Repositories;
using VitalSync.PatientService.Infrastructure.Persistence;

namespace VitalSync.PatientService.Infrastructure.Repositories
{
    public class TriageRecordRepository : ITriageRecordRepository
    {
        private readonly PatientDbContext _context;
        public TriageRecordRepository(PatientDbContext context)
        {
            _context = context;
        }

        public async Task<TriageRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.TriageRecords
                .Include(t => t.Patient)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<TriageRecord>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _context.TriageRecords
                .Where(t => t.PatientId == patientId)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(TriageRecord triageRecord, CancellationToken cancellationToken = default)
        {
            await _context.TriageRecords.AddAsync(triageRecord, cancellationToken);
        }

        public Task UpdateAsync(TriageRecord triageRecord, CancellationToken cancellationToken = default)
        {
            _context.TriageRecords.Update(triageRecord);
            return Task.CompletedTask;
        }
        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }


    }
}
