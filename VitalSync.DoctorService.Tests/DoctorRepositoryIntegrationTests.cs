using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VitalSync.DoctorService.Domain.Entities;
using VitalSync.DoctorService.Infrastructure.Persistence;
using VitalSync.DoctorService.Infrastructure.Repositories;
using Xunit;

namespace VitalSync.DoctorService.Tests.Integration
{
    public class DoctorRepositoryIntegrationTests
    {
        private DoctorDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<DoctorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new DoctorDbContext(options);
        }

        [Fact]
        public async Task AddDoctorAndReserveSlot_PersistsCorrectly()
        {
            using var context = GetDbContext();
            var repo = new DoctorRepository(context);

            var doctor = new Doctor("Dr. House", "Diagnostics", "LIC-001");
            await context.Doctors.AddAsync(doctor);
            await context.SaveChangesAsync();

            var availableDoc = await repo.GetAvailableDoctorBySpecialtyAsync("Diagnostics");
            Assert.NotNull(availableDoc);
            Assert.Equal("Dr. House", availableDoc.FullName);

            var slot = new DoctorSlot(availableDoc.Id, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
            await repo.AddSlotAsync(slot);
            await repo.SaveChangesAsync();

            var retrievedSlot = await repo.GetSlotByIdAsync(slot.Id);
            Assert.NotNull(retrievedSlot);
            Assert.Equal("Reserved", retrievedSlot.Status);
        }
    }
}
