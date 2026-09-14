using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VitalSync.SagaOrchestrator.Application.DTOs;
using VitalSync.SagaOrchestrator.Application.HttpClients;
using VitalSync.SagaOrchestrator.Application.Services;
using VitalSync.SagaOrchestrator.Domain.Entities;
using VitalSync.SagaOrchestrator.Domain.Repositories;
using Xunit;

namespace VitalSync.SagaOrchestrator.Tests
{
    public class EmergencyReservationSagaManagerTests
    {
        private readonly Mock<ISagaRepository> _sagaRepoMock = new Mock<ISagaRepository>();
        private readonly Mock<IDoctorServiceClient> _doctorClientMock = new Mock<IDoctorServiceClient>();
        private readonly Mock<ILabServiceClient> _labClientMock = new Mock<ILabServiceClient>();
        private readonly Mock<ILogger<EmergencyReservationSagaManager>> _loggerMock = new Mock<ILogger<EmergencyReservationSagaManager>>();
        private readonly EmergencyReservationSagaManager _manager;

        public EmergencyReservationSagaManagerTests()
        {
            _manager = new EmergencyReservationSagaManager(
                _sagaRepoMock.Object,
                _doctorClientMock.Object,
                _labClientMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteSagaAsync_AllStepsSucceed_CompletesSaga()
        {
            var dto = new EmergencyReservationSagaDto(Guid.NewGuid(), Guid.NewGuid(), "Cardiology", "MRI");

            _sagaRepoMock.Setup(r => r.AddAsync(It.IsAny<SagaState>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _doctorClientMock.Setup(d => d.ReserveDoctorSlotAsync(dto.TriageId, dto.PatientId, dto.RequiredSpecialty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DoctorReservationResult(true, Guid.NewGuid(), Guid.NewGuid(), "Dr. Smith", string.Empty));

            _labClientMock.Setup(l => l.ReserveLabSlotAsync(dto.TriageId, dto.PatientId, dto.RequiredEquipmentType, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LabReservationResult(true, Guid.NewGuid(), Guid.NewGuid(), "MRI 01", string.Empty));

            var response = await _manager.ExecuteSagaAsync(dto, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Equal("Completed", response.Status);
            Assert.False(response.CompensatingRollbackExecuted);
        }

        [Fact]
        public async Task ExecuteSagaAsync_DoctorReservationFails_FailsSagaWithoutCompensating()
        {
            var dto = new EmergencyReservationSagaDto(Guid.NewGuid(), Guid.NewGuid(), "Neurology", "CT");

            _doctorClientMock.Setup(d => d.ReserveDoctorSlotAsync(dto.TriageId, dto.PatientId, dto.RequiredSpecialty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DoctorReservationResult(false, Guid.Empty, Guid.Empty, string.Empty, "No doctor available"));

            var response = await _manager.ExecuteSagaAsync(dto, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Equal("Failed", response.Status);
            Assert.False(response.CompensatingRollbackExecuted);
        }

        [Fact]
        public async Task ExecuteSagaAsync_LabReservationFails_TriggersCompensatingRollback()
        {
            var dto = new EmergencyReservationSagaDto(Guid.NewGuid(), Guid.NewGuid(), "Orthopedics", "XRay");
            var doctorResId = Guid.NewGuid();

            _doctorClientMock.Setup(d => d.ReserveDoctorSlotAsync(dto.TriageId, dto.PatientId, dto.RequiredSpecialty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DoctorReservationResult(true, doctorResId, Guid.NewGuid(), "Dr. House", string.Empty));

            _labClientMock.Setup(l => l.ReserveLabSlotAsync(dto.TriageId, dto.PatientId, dto.RequiredEquipmentType, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LabReservationResult(false, Guid.Empty, Guid.Empty, string.Empty, "XRay offline"));

            _doctorClientMock.Setup(d => d.ReleaseDoctorSlotAsync(doctorResId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var response = await _manager.ExecuteSagaAsync(dto, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Equal("RolledBack", response.Status);
            Assert.True(response.CompensatingRollbackExecuted);
        }
    }
}