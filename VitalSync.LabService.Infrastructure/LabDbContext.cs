using Microsoft.EntityFrameworkCore;
using VitalSync.LabService.Domain.Entities;

namespace VitalSync.LabService.Infrastructure.Persistence
{
    public class LabDbContext : DbContext
    {
        public LabDbContext(DbContextOptions<LabDbContext> options) : base(options) { }

        public DbSet<LabEquipment> Equipment => Set<LabEquipment>();
        public DbSet<LabSlot> LabSlots => Set<LabSlot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LabEquipment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EquipmentName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EquipmentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RoomNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<LabSlot>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Status).HasMaxLength(50);
                entity.HasOne(s => s.Equipment)
                      .WithMany(e => e.Slots)
                      .HasForeignKey(s => s.EquipmentId);
            });
        }
    }
}
