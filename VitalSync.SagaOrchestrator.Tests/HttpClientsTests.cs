using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using VitalSync.SagaOrchestrator.Infrastructure.HttpClients;
using Xunit;

namespace VitalSync.SagaOrchestrator.Tests
{
    public class HttpClientsTests
    {
        [Fact]
        public async Task DoctorServiceClient_ReserveDoctorSlot_Success_ReturnsSuccessResult()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var reservationId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
            var jsonResponse = $"{{\"reservationId\":\"{reservationId}\",\"doctorId\":\"{doctorId}\",\"doctorName\":\"Dr. House\"}}";

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
            var loggerMock = new Mock<ILogger<DoctorServiceClient>>();
            var client = new DoctorServiceClient(httpClient, loggerMock.Object);

            var result = await client.ReserveDoctorSlotAsync(Guid.NewGuid(), Guid.NewGuid(), "Neurology");

            Assert.True(result.Success);
            Assert.Equal(reservationId, result.ReservationId);
            Assert.Equal(doctorId, result.DoctorId);
            Assert.Equal("Dr. House", result.DoctorName);
        }

        [Fact]
        public async Task DoctorServiceClient_ReleaseDoctorSlot_Success_ReturnsTrue()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
            var loggerMock = new Mock<ILogger<DoctorServiceClient>>();
            var client = new DoctorServiceClient(httpClient, loggerMock.Object);

            var result = await client.ReleaseDoctorSlotAsync(Guid.NewGuid(), "Cancelled");

            Assert.True(result);
        }

        [Fact]
        public async Task LabServiceClient_ReserveLabSlot_Success_ReturnsSuccessResult()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var reservationId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();
            var jsonResponse = $"{{\"reservationId\":\"{reservationId}\",\"equipmentId\":\"{equipmentId}\",\"equipmentName\":\"MRI Scanner 3T\"}}";

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
            var loggerMock = new Mock<ILogger<LabServiceClient>>();
            var client = new LabServiceClient(httpClient, loggerMock.Object);

            var result = await client.ReserveLabSlotAsync(Guid.NewGuid(), Guid.NewGuid(), "MRI");

            Assert.True(result.Success);
            Assert.Equal(reservationId, result.ReservationId);
            Assert.Equal(equipmentId, result.EquipmentId);
            Assert.Equal("MRI Scanner 3T", result.EquipmentName);
        }

        [Fact]
        public async Task LabServiceClient_ReleaseLabSlot_Success_ReturnsTrue()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
            var loggerMock = new Mock<ILogger<LabServiceClient>>();
            var client = new LabServiceClient(httpClient, loggerMock.Object);

            var result = await client.ReleaseLabSlotAsync(Guid.NewGuid(), "Cancelled");

            Assert.True(result);
        }
    }
}
