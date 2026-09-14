using System;
using VitalSync.DoctorService.Domain.Entities;
using Xunit;

namespace VitalSync.DoctorService.Tests
{
    public class DoctorDomainTests
    {
        [Fact]
        public void Doctor_Constructor_InitializesPropertiesCorrectly()
        {
            var doctor = new Doctor("Dr. Gregory House", "Cardiology", "LIC-12345");

            Assert.NotEqual(Guid.Empty, doctor.Id);
            Assert.Equal("Dr. Gregory House", doctor.FullName);
            Assert.Equal("Cardiology", doctor.Specialty);
            Assert.Equal("LIC-12345", doctor.LicenseNumber);
            Assert.True(doctor.IsOnDuty);
        }

        [Fact]
        public void DoctorSlot_ReleaseSlot_UpdatesStatusToReleased()
        {
            var slot = new DoctorSlot(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
            Assert.Equal("Reserved", slot.Status);

            slot.ReleaseSlot("Patient cancelled");

            Assert.Equal("Released", slot.Status);
        }
    }
}