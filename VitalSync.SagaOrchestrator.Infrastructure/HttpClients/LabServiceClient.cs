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
    public class LabServiceClient : ILabServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LabServiceClient> _logger;

        public LabServiceClient(HttpClient httpClient, ILogger<LabServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<LabReservationResult> ReserveLabSlotAsync(Guid triageId, Guid patientId, string equipmentType, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new { TriageId = triageId, PatientId = patientId, EquipmentType = equipmentType };
                var response = await _httpClient.PostAsJsonAsync("/api/v1/Labs/reserve-slot", payload, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    return new LabReservationResult(false, Guid.Empty, Guid.Empty, string.Empty, $"HTTP {(int)response.StatusCode}: {errContent}");
                }

                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
                var root = doc.RootElement;
                var reservationId = root.GetProperty("reservationId").GetGuid();
                var equipmentId = root.GetProperty("equipmentId").GetGuid();
                var equipmentName = root.GetProperty("equipmentName").GetString() ?? "Unknown";

                return new LabReservationResult(true, reservationId, equipmentId, equipmentName, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving lab slot");
                return new LabReservationResult(false, Guid.Empty, Guid.Empty, string.Empty, ex.Message);
            }
        }

        public async Task<bool> ReleaseLabSlotAsync(Guid reservationId, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new { ReservationId = reservationId, Reason = reason };
                var response = await _httpClient.PostAsJsonAsync("/api/v1/Labs/release-slot", payload, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lab slot {ReservationId}", reservationId);
                return false;
            }
        }
    }
}
