using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VitalSync.PatientService.Application.DTOs;
using VitalSync.PatientService.Application.Services;
using VitalSync.PatientService.Domain.Entities;
using VitalSync.PatientService.Domain.Repositories;
using Xunit;

namespace VitalSync.PatientService.Tests
{
    public class PatientAppServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepoMock;
        private readonly PatientAppService _service;

        public PatientAppServiceTests()
        {
            _patientRepoMock = new Mock<IPatientRepository>();
            _service = new PatientAppService(_patientRepoMock.Object);
        }

        [Fact]
        public async Task RegisterPatientAsync_WithValidDto_ReturnsCreatedPatientDto()
        {
            var dto = new CreatePatientDto
            {
                MedicalRecordNumber = "MRN12345",
                FullName = "John Doe",
                Age = 34,
                Gender = "Male",
                BloodGroup = "O+",
                KnownAllergies = "None",
                MedicalHistory = "None"
            };

            _patientRepoMock.Setup(r => r.GetByMrnAsync(dto.MedicalRecordNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            _patientRepoMock.Setup(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _patientRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.RegisterPatientAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("MRN12345", result.MedicalRecordNumber);
            Assert.Equal("John Doe", result.FullName);
            _patientRepoMock.Verify(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Once);
            _patientRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterPatientAsync_DuplicateMrn_ThrowsInvalidOperationException()
        {
            var dto = new CreatePatientDto
            {
                MedicalRecordNumber = "MRN99999",
                FullName = "Alice Smith"
            };

            var existingPatient = new Patient("MRN99999", "Alice Smith", 29, "Female", "A+", "None", "None");

            _patientRepoMock.Setup(r => r.GetByMrnAsync(dto.MedicalRecordNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPatient);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterPatientAsync(dto));
            _patientRepoMock.Verify(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetPatientByIdAsync_ExistingId_ReturnsPatientDto()
        {
            var patient = new Patient("MRN001", "Bob Marley", 45, "Male", "B+", "Pollen", "Hypertension");
            _patientRepoMock.Setup(r => r.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var result = await _service.GetPatientByIdAsync(patient.Id);

            Assert.NotNull(result);
            Assert.Equal(patient.Id, result.Id);
            Assert.Equal("MRN001", result.MedicalRecordNumber);
            Assert.Equal("Bob Marley", result.FullName);
        }

        [Fact]
        public async Task GetPatientByIdAsync_NonExistingId_ReturnsNull()
        {
            var id = Guid.NewGuid();
            _patientRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            var result = await _service.GetPatientByIdAsync(id);

            Assert.Null(result);
        }
    }
}