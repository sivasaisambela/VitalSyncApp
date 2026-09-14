using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VitalSync.PatientService.Application.DTOs;
using VitalSync.PatientService.Application.Services;
using VitalSync.PatientService.Domain.Entities;
using VitalSync.PatientService.Domain.Enums;
using VitalSync.PatientService.Domain.Repositories;
using Xunit;

namespace VitalSync.PatientService.Tests
{
    public class TriageAppServiceTests
    {
        private readonly Mock<ITriageRecordRepository> _triageRepoMock;
        private readonly Mock<IPatientRepository> _patientRepoMock;
        private readonly TriageAppService _service;

        public TriageAppServiceTests()
        {
            _triageRepoMock = new Mock<ITriageRecordRepository>();
            _patientRepoMock = new Mock<IPatientRepository>();
            _service = new TriageAppService(_triageRepoMock.Object, _patientRepoMock.Object);
        }

        [Fact]
        public async Task CreateTriageAssessmentAsync_ExistingPatient_CreatesAndReturnsDto()
        {
            var patientId = Guid.NewGuid();
            var patient = new Patient("MRN100", "Sam Altman", 38, "Male", "O+", "None", "None");

            _patientRepoMock.Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            _triageRepoMock.Setup(r => r.AddAsync(It.IsAny<TriageRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _triageRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var dto = new CreateTriageDto
            {
                PatientId = patientId,
                ReportedSymptoms = "Chest Pain",
                BloodPressure = "140/90",
                HeartRateBpm = 110,
                OxygenSaturationPercent = 95,
                Priority = TriagePriority.Emergency
            };

            var result = await _service.CreateTriageAssessmentAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(patientId, result.PatientId);
            Assert.Equal("Chest Pain", result.ReportedSymptoms);
            Assert.Equal(TriagePriority.Emergency, result.Priority);
            _triageRepoMock.Verify(r => r.AddAsync(It.IsAny<TriageRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateTriageAssessmentAsync_NonExistingPatient_ThrowsKeyNotFoundException()
        {
            var dto = new CreateTriageDto
            {
                PatientId = Guid.NewGuid(),
                ReportedSymptoms = "Headache"
            };

            _patientRepoMock.Setup(r => r.GetByIdAsync(dto.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateTriageAssessmentAsync(dto));
        }

        [Fact]
        public async Task UpdateTriageStatusAsync_ExistingRecord_UpdatesStatusAndReturnsTrue()
        {
            var recordId = Guid.NewGuid();
            var record = new TriageRecord(Guid.NewGuid(), "Fever", "120/80", 80, 99, TriagePriority.Urgent);

            _triageRepoMock.Setup(r => r.GetByIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);

            _triageRepoMock.Setup(r => r.UpdateAsync(record, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _triageRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var success = await _service.UpdateTriageStatusAsync(recordId, "InProgress");

            Assert.True(success);
            Assert.Equal("InProgress", record.Status);
        }

        [Fact]
        public async Task UpdateTriageStatusAsync_NonExistingRecord_ReturnsFalse()
        {
            var recordId = Guid.NewGuid();
            _triageRepoMock.Setup(r => r.GetByIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TriageRecord?)null);

            var success = await _service.UpdateTriageStatusAsync(recordId, "Completed");

            Assert.False(success);
        }
    }
}