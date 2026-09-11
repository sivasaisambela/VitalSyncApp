using System;

namespace VitalSync.DoctorService.Domain.Entities
{
    public class DoctorSlot
    {
        public Guid Id { get; private set; }
        public Guid DoctorId { get; private set; }
        public Guid TriageId { get; private set; }
        public Guid PatientId { get; private set; }
        public DateTime ScheduledTime { get; private set; }
        public string Status { get; private set; } = "Reserved";
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public Doctor? Doctor { get; private set; }

        public DoctorSlot(Guid doctorId, Guid triageId, Guid patientId, DateTime scheduledTime)
        {
            Id = Guid.NewGuid();
            DoctorId = doctorId;
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

        private DoctorSlot() { }
    }
}
