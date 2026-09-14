using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VitalSync.PatientService.Api.Controllers;
using VitalSync.PatientService.Application.DTOs;
using VitalSync.PatientService.Application.Services;
using Xunit;

namespace VitalSync.PatientService.Tests
{
    public class PatientsControllerTests
    {
        private readonly Mock<IPatientAppService> _serviceMock;
        private readonly PatientsController _controller;

        public PatientsControllerTests()
        {
            _serviceMock = new Mock<IPatientAppService>();
            _controller = new PatientsController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            var list = new List<PatientDto>
            {
                new PatientDto { Id = Guid.NewGuid(), MedicalRecordNumber = "MRN1" }
            };
            _serviceMock.Setup(s => s.GetAllPatientsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var result = await _controller.GetAll(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<PatientDto>>(okResult.Value);
            Assert.Single(items);
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            var id = Guid.NewGuid();
            var dto = new PatientDto { Id = id, MedicalRecordNumber = "MRN1" };
            _serviceMock.Setup(s => s.GetPatientByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

            var result = await _controller.GetById(id, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(dto, okResult.Value);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetPatientByIdAsync( id, It.IsAny<CancellationToken>())).ReturnsAsync((PatientDto?)null);

            var result = await _controller.GetById(id, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}