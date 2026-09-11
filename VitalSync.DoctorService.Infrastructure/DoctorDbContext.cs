using Microsoft.EntityFrameworkCore;
using VitalSync.DoctorService.Domain.Entities;

namespace VitalSync.DoctorService.Infrastructure.Persistence
{
    public class DoctorDbContext : DbContext
    {
        public DoctorDbContext(DbContextOptions<DoctorDbContext> options) : base(options) { }

        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<DoctorSlot> DoctorSlots => Set<DoctorSlot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.FullName).IsRequired().HasMaxLength(150);
                entity.Property(d => d.Specialty).IsRequired().HasMaxLength(100);
                entity.Property(d => d.LicenseNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(d => d.LicenseNumber).IsUnique();
            });

            modelBuilder.Entity<DoctorSlot>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Status).HasMaxLength(50);
                entity.HasOne(s => s.Doctor)
                      .WithMany(d => d.Slots)
                      .HasForeignKey(s => s.DoctorId);
            });
        }
    }
}
