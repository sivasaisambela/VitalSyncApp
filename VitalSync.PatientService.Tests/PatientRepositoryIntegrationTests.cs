using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VitalSync.PatientService.Domain.Entities;
using VitalSync.PatientService.Domain.Enums;
using VitalSync.PatientService.Infrastructure.Persistence;
using VitalSync.PatientService.Infrastructure.Repositories;
using Xunit;

namespace VitalSync.PatientService.Tests.Integration
{
    public class PatientRepositoryIntegrationTests
    {
        private PatientDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<PatientDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PatientDbContext(options);
        }

        [Fact]
        public async Task AddAndGetPatient_PersistsToInMemoryDatabase()
        {
            using var context = GetDbContext();
            var repo = new PatientRepository(context);

            var patient = new Patient("MRN-101", "Jane Doe", 32, "Female", "O+", "Pollen", "Hypertension");
            await repo.AddAsync(patient);
            await repo.SaveChangesAsync();

            var retrieved = await repo.GetByIdAsync(patient.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("MRN-101", retrieved.MedicalRecordNumber);
            Assert.Equal("Jane Doe", retrieved.FullName);
        }

        [Fact]
        public async Task TriageRecordRepository_AddAndQueryByPatientId_ReturnsRecordsOrdered()
        {
            using var context = GetDbContext();
            var patientRepo = new PatientRepository(context);
            var triageRepo = new TriageRecordRepository(context);

            var patient = new Patient("MRN-202", "John Smith", 45, "Male", "A+", "None", "None");
            await patientRepo.AddAsync(patient);
            await patientRepo.SaveChangesAsync();

            var record1 = new TriageRecord(patient.Id, "Chest pain", "140/90", 110, 95, TriagePriority.Urgent);
            var record2 = new TriageRecord(patient.Id, "Shortness of breath", "160/100", 130, 88, TriagePriority.Emergency);

            await triageRepo.AddAsync(record1);
            await triageRepo.AddAsync(record2);
            await triageRepo.SaveChangesAsync();

            var records = (await triageRepo.GetByPatientIdAsync(patient.Id)).ToList();

            Assert.Equal(2, records.Count);
            Assert.Contains(records, r => r.ReportedSymptoms == "Chest pain");
            Assert.Contains(records, r => r.ReportedSymptoms == "Shortness of breath");
        }
    }
}
