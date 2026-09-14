using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VitalSync.SagaOrchestrator.Domain.Entities;
using VitalSync.SagaOrchestrator.Domain.Repositories;

namespace VitalSync.SagaOrchestrator.Infrastructure.Persistence
{
    public class SagaRepository : ISagaRepository
    {
        private readonly SagaDbContext _context;

        public SagaRepository(SagaDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SagaState saga, CancellationToken cancellationToken = default)
        {
            await _context.Sagas.AddAsync(saga, cancellationToken);
        }

        public async Task<SagaState?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Sagas.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
