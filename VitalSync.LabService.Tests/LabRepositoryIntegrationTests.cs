using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VitalSync.LabService.Domain.Entities;
using VitalSync.LabService.Infrastructure.Persistence;
using VitalSync.LabService.Infrastructure.Repositories;
using Xunit;

namespace VitalSync.LabService.Tests.Integration
{
    public class LabRepositoryIntegrationTests
    {
        private LabDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<LabDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new LabDbContext(options);
        }

        [Fact]
        public async Task AddEquipmentAndReserveSlot_PersistsAndUpdatesStatus()
        {
            using var context = GetDbContext();
            var repo = new LabRepository(context);

            var equipment = new LabEquipment("MRI Scanner 3T", "MRI", "Room 102");
            await context.Equipment.AddAsync(equipment);
            await context.SaveChangesAsync();

            var availableEq = await repo.GetAvailableEquipmentByTypeAsync("MRI");
            Assert.NotNull(availableEq);
            Assert.Equal("MRI Scanner 3T", availableEq.EquipmentName);

            var slot = new LabSlot(availableEq.Id, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
            await repo.AddSlotAsync(slot);
            await repo.SaveChangesAsync();

            var retrievedSlot = await repo.GetSlotByIdAsync(slot.Id);
            Assert.NotNull(retrievedSlot);
            Assert.Equal("Reserved", retrievedSlot.Status);

            retrievedSlot.ReleaseSlot("Completed");
            await repo.SaveChangesAsync();

            var updatedSlot = await repo.GetSlotByIdAsync(slot.Id);
            Assert.NotNull(updatedSlot);
            Assert.Equal("Released", updatedSlot.Status);
        }
    }
}
