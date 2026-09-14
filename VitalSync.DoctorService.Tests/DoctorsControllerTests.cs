using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VitalSync.DoctorService.Api.Controllers;
using VitalSync.DoctorService.Domain.Entities;
using VitalSync.DoctorService.Domain.Repositories;
using Xunit;

namespace VitalSync.DoctorService.Tests
{
    public class DoctorsControllerTests
    {
        private readonly Mock<IDoctorRepository> _repoMock;
        private readonly Mock<ILogger<DoctorsController>> _loggerMock;
        private readonly DoctorsController _controller;

        public DoctorsControllerTests()
        {
            _repoMock = new Mock<IDoctorRepository>();
            _loggerMock = new Mock<ILogger<DoctorsController>>();
            _controller = new DoctorsController(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ReserveSlot_AvailableDoctor_ReturnsOkResult()
        {
            var doctor = new Doctor("Dr. Strange", "Neurology", "LIC-777");
            _repoMock.Setup(r => r.GetAvailableDoctorBySpecialtyAsync("Neurology", It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _repoMock.Setup(r => r.AddSlotAsync(It.IsAny<DoctorSlot>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var req = new ReserveDoctorSlotRequest(Guid.NewGuid(), Guid.NewGuid(), "Neurology");
            var result = await _controller.ReserveSlot(req, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _repoMock.Verify(r => r.AddSlotAsync(It.IsAny<DoctorSlot>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReserveSlot_NoDoctorAvailable_ReturnsNotFound()
        {
            _repoMock.Setup(r => r.GetAvailableDoctorBySpecialtyAsync("Pediatrics", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            var req = new ReserveDoctorSlotRequest(Guid.NewGuid(), Guid.NewGuid(), "Pediatrics");
            var result = await _controller.ReserveSlot(req, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ReleaseSlot_ExistingSlot_ReturnsOk()
        {
            var slotId = Guid.NewGuid();
            var slot = new DoctorSlot(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

            _repoMock.Setup(r => r.GetSlotByIdAsync(slotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(slot);

            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var req = new ReleaseDoctorSlotRequest(slotId, "Completed");
            var result = await _controller.ReleaseSlot(req, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Released", slot.Status);
        }

        [Fact]
        public async Task ReleaseSlot_NonExistingSlot_ReturnsNotFound()
        {
            var slotId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetSlotByIdAsync(slotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DoctorSlot?)null);

            var req = new ReleaseDoctorSlotRequest(slotId, "Cancel");
            var result = await _controller.ReleaseSlot(req, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
