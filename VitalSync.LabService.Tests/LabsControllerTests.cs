using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VitalSync.LabService.Api.Controllers;
using VitalSync.LabService.Domain.Entities;
using VitalSync.LabService.Domain.Repositories;
using Xunit;

namespace VitalSync.LabService.Tests
{
    public class LabsControllerTests
    {
        private readonly Mock<ILabRepository> _repoMock;
        private readonly Mock<ILogger<LabsController>> _loggerMock;
        private readonly LabsController _controller;

        public LabsControllerTests()
        {
            _repoMock = new Mock<ILabRepository>();
            _loggerMock = new Mock<ILogger<LabsController>>();
            _controller = new LabsController(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ReserveSlot_AvailableEquipment_ReturnsOkResult()
        {
            var equipment = new LabEquipment("CT Scanner A", "CT", "Room 202");
            _repoMock.Setup(r => r.GetAvailableEquipmentByTypeAsync("CT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(equipment);

            _repoMock.Setup(r => r.AddSlotAsync(It.IsAny<LabSlot>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var req = new ReserveLabSlotRequest(Guid.NewGuid(), Guid.NewGuid(), "CT");
            var result = await _controller.ReserveSlot(req, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _repoMock.Verify(r => r.AddSlotAsync(It.IsAny<LabSlot>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReserveSlot_NoEquipmentAvailable_ReturnsNotFound()
        {
            _repoMock.Setup(r => r.GetAvailableEquipmentByTypeAsync("XRay", It.IsAny<CancellationToken>()))
                .ReturnsAsync((LabEquipment?)null);

            var req = new ReserveLabSlotRequest(Guid.NewGuid(), Guid.NewGuid(), "XRay");
            var result = await _controller.ReserveSlot(req, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ReleaseSlot_ExistingSlot_ReturnsOk()
        {
            var slotId = Guid.NewGuid();
            var slot = new LabSlot(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

            _repoMock.Setup(r => r.GetSlotByIdAsync(slotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(slot);

            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var req = new ReleaseLabSlotRequest(slotId, "Compensate");
            var result = await _controller.ReleaseSlot(req, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Released", slot.Status);
        }

        [Fact]
        public async Task ReleaseSlot_NonExistingSlot_ReturnsNotFound()
        {
            var slotId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetSlotByIdAsync(slotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((LabSlot?)null);

            var req = new ReleaseLabSlotRequest(slotId, "Unknown");
            var result = await _controller.ReleaseSlot(req, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}