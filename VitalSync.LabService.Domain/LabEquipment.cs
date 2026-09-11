using System;
using System.Collections.Generic;

namespace VitalSync.LabService.Domain.Entities
{
    public class LabEquipment
    {
        public Guid Id { get; private set; }
        public string EquipmentName { get; private set; } = string.Empty;
        public string EquipmentType { get; private set; } = string.Empty;
        public string RoomNumber { get; private set; } = string.Empty;
        public bool IsOperational { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public ICollection<LabSlot> Slots { get; private set; } = new List<LabSlot>();

        public LabEquipment(string equipmentName, string equipmentType, string roomNumber)
        {
            Id = Guid.NewGuid();
            EquipmentName = equipmentName;
            EquipmentType = equipmentType;
            RoomNumber = roomNumber;
            IsOperational = true;
            CreatedAt = DateTime.UtcNow;
        }

        private LabEquipment() { }
    }
}
