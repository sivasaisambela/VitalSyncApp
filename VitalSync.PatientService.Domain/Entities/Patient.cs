using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalSync.PatientService.Domain.Entities
{
    public class Patient
    {
        public Guid Id { get; private set; }
        public string MedicalRecordNumber { get; private set; } = string.Empty;
        public string FullName { get; private set; } = string.Empty;
        public int Age { get; private set; }
        public string Gender { get; private set; } = string.Empty;
        public string BloodGroup { get; private set; } = string.Empty;
        public string KnownAllergies { get; private set; } = string.Empty;
        public string MedicalHistory { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public ICollection<TriageRecord> TriageRecords { get; private set; } = new List<TriageRecord>();
        public Patient(string medicalRecordNumber, string fullName, int age, string gender, string bloodGroup, string knownAllergies, string medicalHistory)
        {
            Id = Guid.NewGuid();
            MedicalRecordNumber = medicalRecordNumber;
            FullName = fullName;
            Age = age;
            Gender = gender;
            BloodGroup = bloodGroup;
            KnownAllergies = knownAllergies;
            MedicalHistory = medicalHistory;
            CreatedAt = DateTime.UtcNow;
        }
        private Patient() { }
    }
}
