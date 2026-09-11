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
    public class PatientRepository : IPatientRepository
    {
        private readonly PatientDbContext _context;
        public PatientRepository(PatientDbContext context)
        {
            _context = context;
        }

        public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Patients
                .Include(p => p.TriageRecords)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Patient?> GetByMrnAsync(string medicalRecordNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Patients
                .Include(p => p.TriageRecords)
                .FirstOrDefaultAsync(p => p.MedicalRecordNumber == medicalRecordNumber, cancellationToken);
        }
        public async Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Patients
                .Include(p => p.TriageRecords)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            await _context.Patients.AddAsync(patient, cancellationToken);
        }
        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }

    }
}
