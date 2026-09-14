using System;
using VitalSync.LabService.Domain.Entities;
using Xunit;

namespace VitalSync.LabService.Tests
{
    public class LabDomainTests
    {
        [Fact]
        public void LabEquipment_Constructor_InitializesPropertiesCorrectly()
        {
            var equipment = new LabEquipment("MRI Scanner X1", "MRI", "Room 101");
            Assert.NotEqual(Guid.Empty, equipment.Id);
            Assert.Equal("MRI Scanner X1", equipment.EquipmentName);
            Assert.Equal("MRI", equipment.EquipmentType);
            Assert.Equal("Room 101", equipment.RoomNumber);
            Assert.True(equipment.IsOperational);
        }

        [Fact]
        public void LabSlot_ReleaseSlot_UpdatesStatusToReleased()
        {
            var slot = new LabSlot(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
            Assert.Equal("Reserved", slot.Status);

            slot.ReleaseSlot("Rollback");

            Assert.Equal("Released", slot.Status);
        }
    }
}