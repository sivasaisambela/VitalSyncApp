using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Domain.Enums;

namespace VitalSync.PatientService.Domain.Entities
{
    public class TriageRecord
    {
        public Guid Id { get; private set; }
        public Guid PatientId { get; private set; }
        public string ReportedSymptoms { get; private set; } = string.Empty;
        public string BloodPressure { get; private set; } = string.Empty;
        public int HeartRateBpm { get; private set; }
        public int OxygenSaturationPercent { get; private set; }
        public TriagePriority Priority { get; private set; }
        public string Status { get; private set; } = "Pending";
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Patient? Patient { get; private set; }
        public TriageRecord(Guid patientId, string reportedSymptoms, string bloodPressure, int heartRateBpm, int oxygenSaturationPercent, TriagePriority priority)
        {
            Id = Guid.NewGuid();
            PatientId = patientId;
            ReportedSymptoms = reportedSymptoms;
            BloodPressure = bloodPressure;
            HeartRateBpm = heartRateBpm;
            OxygenSaturationPercent = oxygenSaturationPercent;
            Priority = priority;
            Status = "Pending";
            CreatedAt = DateTime.UtcNow;
        }
        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
        }
        private TriageRecord() { }
    }
}
