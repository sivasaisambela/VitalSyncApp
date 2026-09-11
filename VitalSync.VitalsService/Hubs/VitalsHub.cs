using Microsoft.AspNetCore.SignalR;
using VitalSync.VitalsService.DTOs;

namespace VitalSync.VitalsService.Hubs
{
    public class VitalsHub : Hub<IVitalsClient>
    {
        private readonly ILogger<VitalsHub> _logger;
        public VitalsHub(ILogger<VitalsHub> logger)
        {
            _logger = logger;
        }
        // Called by Bedside Monitors to broadcast vitals
        public async Task SendVitalsTelemetry(VitalsTelemetryDto telemetry)
        {
            _logger.LogInformation("Vitals received for Patient {PatientId}: HR={HR}, SpO2={SpO2}",
                telemetry.PatientId, telemetry.HeartRateBpm, telemetry.OxygenSaturationPercent);
            // Broadcast to all connected Nurse Station dashboards
            await Clients.All.ReceiveVitalsUpdate(telemetry);
            // If critical alert condition met, send high-priority alert event
            if (telemetry.IsCriticalAlert)
            {
                _logger.LogWarning("CRITICAL ALERT triggered for Patient {PatientId}!", telemetry.PatientId);
                await Clients.All.ReceiveCriticalAlert(telemetry);
            }
        }
    }
}
