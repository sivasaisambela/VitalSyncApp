using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Domain.Entities;

namespace VitalSync.PatientService.Domain.Repositories
{
    public interface ITriageRecordRepository
    {
        Task<TriageRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TriageRecord>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task AddAsync(TriageRecord triageRecord, CancellationToken cancellationToken = default);
        Task UpdateAsync(TriageRecord triageRecord, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
