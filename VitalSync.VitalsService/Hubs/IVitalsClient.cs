using VitalSync.VitalsService.DTOs;

namespace VitalSync.VitalsService.Hubs
{
    // Strongly-Typed SignalR Interface
    public interface IVitalsClient
    {
        Task ReceiveVitalsUpdate(VitalsTelemetryDto telemetry);
        Task ReceiveCriticalAlert(VitalsTelemetryDto telemetry);
    }
}
