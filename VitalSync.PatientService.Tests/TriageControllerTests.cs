using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VitalSync.PatientService.Api.Controllers;
using VitalSync.PatientService.Application.DTOs;
using VitalSync.PatientService.Application.Services;
using VitalSync.PatientService.Domain.Enums;
using Xunit;

namespace VitalSync.PatientService.Tests
{
    public class TriageControllerTests
    {
        private readonly Mock<ITriageAppService> _serviceMock;
        private readonly TriageController _controller;

        public TriageControllerTests()
        {
            _serviceMock = new Mock<ITriageAppService>();
            _controller = new TriageController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetById_ExistingRecord_ReturnsOkObjectResult()
        {
            var recordId = Guid.NewGuid();
            var dto = new TriageRecordDto
            {
                Id = recordId,
                PatientId = Guid.NewGuid(),
                ReportedSymptoms = "Fever",
                BloodPressure = "120/80",
                HeartRateBpm = 80,
                OxygenSaturationPercent = 98,
                Priority = TriagePriority.Routine,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _serviceMock.Setup(s => s.GetTriageRecordByIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _controller.GetById(recordId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(dto, okResult.Value);
        }

        [Fact]
        public async Task GetById_NonExistingRecord_ReturnsNotFound()
        {
            var recordId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetTriageRecordByIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TriageRecordDto?)null);

            var result = await _controller.GetById(recordId, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetByPatientId_ReturnsOkList()
        {
            var patientId = Guid.NewGuid();
            var list = new List<TriageRecordDto>
            {
                new TriageRecordDto
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    ReportedSymptoms = "Headache",
                    BloodPressure = "120/80",
                    HeartRateBpm = 75,
                    OxygenSaturationPercent = 99,
                    Priority = TriagePriority.Routine,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                }
            };
            _serviceMock.Setup(s => s.GetTriageRecordsForPatientAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var result = await _controller.GetByPatientId(patientId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(list, okResult.Value);
        }

        [Fact]
        public async Task CreateAssessment_ValidDto_ReturnsCreatedAtAction()
        {
            var createDto = new CreateTriageDto
            {
                PatientId = Guid.NewGuid(),
                ReportedSymptoms = "Chest pain",
                BloodPressure = "140/90",
                HeartRateBpm = 110,
                OxygenSaturationPercent = 92,
                Priority = TriagePriority.Emergency
            };
            var recordDto = new TriageRecordDto
            {
                Id = Guid.NewGuid(),
                PatientId = createDto.PatientId,
                ReportedSymptoms = createDto.ReportedSymptoms,
                BloodPressure = createDto.BloodPressure,
                HeartRateBpm = createDto.HeartRateBpm,
                OxygenSaturationPercent = createDto.OxygenSaturationPercent,
                Priority = createDto.Priority,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _serviceMock.Setup(s => s.CreateTriageAssessmentAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recordDto);

            var result = await _controller.CreateAssessment(createDto, CancellationToken.None);

            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(recordDto, createdAtAction.Value);
        }

        [Fact]
        public async Task CreateAssessment_NonExistingPatient_ReturnsNotFound()
        {
            var createDto = new CreateTriageDto
            {
                PatientId = Guid.NewGuid(),
                ReportedSymptoms = "Chest pain",
                BloodPressure = "140/90",
                HeartRateBpm = 110,
                OxygenSaturationPercent = 92,
                Priority = TriagePriority.Emergency
            };
            _serviceMock.Setup(s => s.CreateTriageAssessmentAsync(createDto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Patient not found"));

            var result = await _controller.CreateAssessment(createDto, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateStatus_Success_ReturnsNoContent()
        {
            var recordId = Guid.NewGuid();
            _serviceMock.Setup(s => s.UpdateTriageStatusAsync(recordId, "Completed", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateStatus(recordId, "Completed", CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_RecordNotFound_ReturnsNotFound()
        {
            var recordId = Guid.NewGuid();
            _serviceMock.Setup(s => s.UpdateTriageStatusAsync(recordId, "Completed", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateStatus(recordId, "Completed", CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
