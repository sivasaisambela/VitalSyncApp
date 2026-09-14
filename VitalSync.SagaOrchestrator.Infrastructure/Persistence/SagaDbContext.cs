using Microsoft.EntityFrameworkCore;
using VitalSync.SagaOrchestrator.Domain.Entities;

namespace VitalSync.SagaOrchestrator.Infrastructure.Persistence
{
    public class SagaDbContext : DbContext
    {
        public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options) { }

        public DbSet<SagaState> Sagas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SagaState>(builder =>
            {
                builder.HasKey(s => s.Id);
                builder.Property(s => s.RequiredSpecialty).HasMaxLength(100).IsRequired();
                builder.Property(s => s.RequiredEquipmentType).HasMaxLength(100).IsRequired();
            });
        }
    }
}
