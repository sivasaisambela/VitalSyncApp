using System;
using System.Collections.Generic;

namespace VitalSync.DoctorService.Domain.Entities
{
    public class Doctor
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public string Specialty { get; private set; } = string.Empty;
        public string LicenseNumber { get; private set; } = string.Empty;
        public bool IsOnDuty { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public ICollection<DoctorSlot> Slots { get; private set; } = new List<DoctorSlot>();

        public Doctor(string fullName, string specialty, string licenseNumber)
        {
            Id = Guid.NewGuid();
            FullName = fullName;
            Specialty = specialty;
            LicenseNumber = licenseNumber;
            IsOnDuty = true;
            CreatedAt = DateTime.UtcNow;
        }

        private Doctor() { }
    }
}
