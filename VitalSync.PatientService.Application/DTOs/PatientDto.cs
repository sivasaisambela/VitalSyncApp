using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalSync.PatientService.Application.DTOs
{
    public class PatientDto
    {
        public Guid Id { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string KnownAllergies { get; set; } = string.Empty;
        public string MedicalHistory { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Added to resolve CS0117
        public List<TriageRecordDto> TriageRecords { get; set; } = new List<TriageRecordDto>();
    }
}
