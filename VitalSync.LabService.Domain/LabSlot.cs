using System;

namespace VitalSync.LabService.Domain.Entities
{
    public class LabSlot
    {
        public Guid Id { get; private set; }
        public Guid EquipmentId { get; private set; }
        public Guid TriageId { get; private set; }
        public Guid PatientId { get; private set; }
        public DateTime ScheduledTime { get; private set; }
        public string Status { get; private set; } = "Reserved";
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public LabEquipment? Equipment { get; private set; }

        public LabSlot(Guid equipmentId, Guid triageId, Guid patientId, DateTime scheduledTime)
        {
            Id = Guid.NewGuid();
            EquipmentId = equipmentId;
            TriageId = triageId;
            PatientId = patientId;
            ScheduledTime = scheduledTime;
            Status = "Reserved";
            CreatedAt = DateTime.UtcNow;
        }

        public void ReleaseSlot(string reason)
        {
            Status = "Released";
        }

        private LabSlot() { }
    }
}
