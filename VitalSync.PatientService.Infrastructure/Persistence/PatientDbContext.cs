using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Domain.Entities;

namespace VitalSync.PatientService.Infrastructure.Persistence
{
    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<TriageRecord> TriageRecords => Set<TriageRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure Patient Entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.MedicalRecordNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(p => p.MedicalRecordNumber).IsUnique();
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Gender).HasMaxLength(20);
                entity.Property(p => p.BloodGroup).HasMaxLength(10);
                // One-to-Many Relationship: Patient has many TriageRecords
                entity.HasMany(p => p.TriageRecords)
                      .WithOne(t => t.Patient)
                      .HasForeignKey(t => t.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Configure TriageRecord Entity
            modelBuilder.Entity<TriageRecord>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ReportedSymptoms).IsRequired().HasMaxLength(1000);
                entity.Property(t => t.BloodPressure).HasMaxLength(20);
                entity.Property(t => t.Status).HasMaxLength(50);
                entity.Property(t => t.Priority).HasConversion<int>();
            });
        }
    
    }
}
