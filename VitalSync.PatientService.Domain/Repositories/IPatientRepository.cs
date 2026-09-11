using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Domain.Entities;

namespace VitalSync.PatientService.Domain.Repositories
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Patient?> GetByMrnAsync(string medicalRecordNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
