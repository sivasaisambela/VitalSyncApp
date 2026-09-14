using System;
using System.Threading;
using System.Threading.Tasks;
using VitalSync.SagaOrchestrator.Domain.Entities;

namespace VitalSync.SagaOrchestrator.Domain.Repositories
{
    public interface ISagaRepository
    {
        Task AddAsync(SagaState saga, CancellationToken cancellationToken = default);
        Task<SagaState?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
