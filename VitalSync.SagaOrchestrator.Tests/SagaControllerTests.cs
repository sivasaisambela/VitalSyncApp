using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VitalSync.SagaOrchestrator.Api.Controllers;
using VitalSync.SagaOrchestrator.Application.DTOs;
using VitalSync.SagaOrchestrator.Application.Services;
using VitalSync.SagaOrchestrator.Domain.Entities;
using Xunit;

namespace VitalSync.SagaOrchestrator.Tests
{
    public class SagaControllerTests
    {
        private readonly Mock<IEmergencyReservationSagaManager> _managerMock;
        private readonly SagaController _controller;

        public SagaControllerTests()
        {
            _managerMock = new Mock<IEmergencyReservationSagaManager>();
            _controller = new SagaController(_managerMock.Object);
        }

        [Fact]
        public async Task TriggerEmergencyReservation_Success_ReturnsOk()
        {
            var dto = new EmergencyReservationSagaDto(Guid.NewGuid(), Guid.NewGuid(), "Cardiology", "MRI");
            var response = new EmergencyReservationSagaResponseDto(
                Guid.NewGuid(), dto.TriageId, dto.PatientId, "Completed", Guid.NewGuid(), "Dr. Smith", Guid.NewGuid(), "MRI 01", false, 120, new List<string>()
            );


            _managerMock.Setup(m => m.ExecuteSagaAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);


            var result = await _controller.TriggerEmergencyReservation(dto, CancellationToken.None);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }


        [Fact]
        public async Task TriggerEmergencyReservation_RolledBack_ReturnsUnprocessableEntity()
        {
            var dto = new EmergencyReservationSagaDto(Guid.NewGuid(), Guid.NewGuid(), "Cardiology", "MRI");
            var response = new EmergencyReservationSagaResponseDto(
                Guid.NewGuid(), dto.TriageId, dto.PatientId, "RolledBack", null, null, null, null, true, 200, new List<string>()
            );


            _managerMock.Setup(m => m.ExecuteSagaAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);


            var result = await _controller.TriggerEmergencyReservation(dto, CancellationToken.None);


            var unprocResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.Equal(response, unprocResult.Value);
        }


        [Fact]
        public async Task GetSagaState_Existing_ReturnsOk()
        {
            var id = Guid.NewGuid();
            var state = new SagaState(Guid.NewGuid(), Guid.NewGuid(), "Cardiology", "MRI");


            _managerMock.Setup(m => m.GetSagaStateAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(state);


            var result = await _controller.GetSagaState(id, CancellationToken.None);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(state, okResult.Value);
        }

        [Fact]
        public async Task GetSagaState_NotFound_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            _managerMock.Setup(m => m.GetSagaStateAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SagaState?)null);


            var result = await _controller.GetSagaState(id, CancellationToken.None);


            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}