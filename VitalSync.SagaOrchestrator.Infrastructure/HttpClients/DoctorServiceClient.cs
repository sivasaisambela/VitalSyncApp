using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitalSync.SagaOrchestrator.Application.DTOs;
using VitalSync.SagaOrchestrator.Application.HttpClients;

namespace VitalSync.SagaOrchestrator.Infrastructure.HttpClients
{
    public class DoctorServiceClient : IDoctorServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DoctorServiceClient> _logger;

        public DoctorServiceClient(HttpClient httpClient, ILogger<DoctorServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DoctorReservationResult> ReserveDoctorSlotAsync(Guid triageId, Guid patientId, string specialty, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new { TriageId = triageId, PatientId = patientId, Specialty = specialty };
                var response = await _httpClient.PostAsJsonAsync("/api/v1/Doctors/reserve-slot", payload, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    return new DoctorReservationResult(false, Guid.Empty, Guid.Empty, string.Empty, $"HTTP {(int)response.StatusCode}: {errContent}");
                }

                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
                var root = doc.RootElement;
                var reservationId = root.GetProperty("reservationId").GetGuid();
                var doctorId = root.GetProperty("doctorId").GetGuid();
                var doctorName = root.GetProperty("doctorName").GetString() ?? "Unknown";

                return new DoctorReservationResult(true, reservationId, doctorId, doctorName, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving doctor slot");
                return new DoctorReservationResult(false, Guid.Empty, Guid.Empty, string.Empty, ex.Message);
            }
        }

        public async Task<bool> ReleaseDoctorSlotAsync(Guid reservationId, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new { ReservationId = reservationId, Reason = reason };
                var response = await _httpClient.PostAsJsonAsync("/api/v1/Doctors/release-slot", payload, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing doctor slot {ReservationId}", reservationId);
                return false;
            }
        }
    }
}
