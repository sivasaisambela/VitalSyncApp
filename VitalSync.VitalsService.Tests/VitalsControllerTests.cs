using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using VitalSync.VitalsService.Controllers;
using VitalSync.VitalsService.DTOs;
using VitalSync.VitalsService.Hubs;
using Xunit;

namespace VitalSync.VitalsService.Tests
{
    public class VitalsControllerTests
    {
        private readonly Mock<IHubContext<VitalsHub, IVitalsClient>> _hubContextMock;
        private readonly Mock<IHubClients<IVitalsClient>> _clientsMock;
        private readonly Mock<IVitalsClient> _clientProxyMock;
        private readonly Mock<ILogger<VitalsController>> _loggerMock;
        private readonly VitalsController _controller;

        public VitalsControllerTests()
        {
            _hubContextMock = new Mock<IHubContext<VitalsHub, IVitalsClient>>();
            _clientsMock = new Mock<IHubClients<IVitalsClient>>();
            _clientProxyMock = new Mock<IVitalsClient>();
            _loggerMock = new Mock<ILogger<VitalsController>>();

            _clientsMock.Setup(s => s.All).Returns(_clientProxyMock.Object);
            _hubContextMock.Setup(h => h.Clients).Returns(_clientsMock.Object);

            _controller = new VitalsController(_hubContextMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task IngestTelemetry_EmptyPatientId_ReturnsBadRequest()
        {
            var telemetry = new VitalsTelemetryDto { PatientId = Guid.Empty };

            var result = await _controller.IngestTelemetry(telemetry);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task IngestTelemetry_NormalVitals_BroadcastsVitalsUpdateAndReturnsOk()
        {
            var telemetry = new VitalsTelemetryDto
            {
                PatientId = Guid.NewGuid(),
                HeartRateBpm = 75,
                OxygenSaturationPercent = 98
            };

            _clientProxyMock.Setup(c => c.ReceiveVitalsUpdate(It.IsAny<VitalsTelemetryDto>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.IngestTelemetry(telemetry);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _clientProxyMock.Verify(c => c.ReceiveVitalsUpdate(telemetry), Times.Once);
            _clientProxyMock.Verify(c => c.ReceiveCriticalAlert(It.IsAny<VitalsTelemetryDto>()), Times.Never);
        }

        [Fact]
        public async Task IngestTelemetry_CriticalVitals_BroadcastsCriticalAlertAndReturnsOk()
        {
            var telemetry = new VitalsTelemetryDto
            {
                PatientId = Guid.NewGuid(),
                HeartRateBpm = 160,
                OxygenSaturationPercent = 85
            };

            _clientProxyMock.Setup(c => c.ReceiveVitalsUpdate(It.IsAny<VitalsTelemetryDto>()))
                .Returns(Task.CompletedTask);

            _clientProxyMock.Setup(c => c.ReceiveCriticalAlert(It.IsAny<VitalsTelemetryDto>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.IngestTelemetry(telemetry);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _clientProxyMock.Verify(c => c.ReceiveVitalsUpdate(telemetry), Times.Once);
            _clientProxyMock.Verify(c => c.ReceiveCriticalAlert(telemetry), Times.Once);
        }
    }
}
