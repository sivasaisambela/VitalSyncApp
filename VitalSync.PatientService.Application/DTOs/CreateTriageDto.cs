using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Domain.Enums;

namespace VitalSync.PatientService.Application.DTOs
{
    public class CreateTriageDto
    {
        public Guid PatientId { get; set; }
        public string ReportedSymptoms { get; set; } = string.Empty;
        public string BloodPressure { get; set; } = string.Empty;
        public int HeartRateBpm { get; set; }
        public int OxygenSaturationPercent { get; set; }
        public TriagePriority Priority { get; set; }
    }
}
