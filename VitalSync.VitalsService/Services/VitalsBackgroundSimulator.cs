using Microsoft.AspNetCore.SignalR;
using VitalSync.VitalsService.DTOs;
using VitalSync.VitalsService.Hubs;

namespace VitalSync.VitalsService.Services
{
    public class VitalsBackgroundSimulator : BackgroundService
    {
        private readonly IHubContext<VitalsHub, IVitalsClient> _hubContext;
        private readonly ILogger<VitalsBackgroundSimulator> _logger;
        private readonly Random _random = new();
        // In-memory register of active patients being monitored in the ER
        private static readonly List<(Guid Id, string Mrn)> ActiveMonitoredPatients = new();
        public VitalsBackgroundSimulator(IHubContext<VitalsHub, IVitalsClient> hubContext, ILogger<VitalsBackgroundSimulator> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }
        // Method to dynamically add patients to the active monitoring stream
        public static void RegisterPatientForMonitoring(Guid patientId, string mrn)
        {
            if (!ActiveMonitoredPatients.Any(p => p.Id == patientId))
            {
                ActiveMonitoredPatients.Add((patientId, mrn));
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dynamic Vitals Telemetry Simulator started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                // If patients are registered, simulate telemetry for all active patients
                foreach (var (patientId, mrn) in ActiveMonitoredPatients.ToList())
                {
                    var telemetry = new VitalsTelemetryDto
                    {
                        PatientId = patientId,
                        MedicalRecordNumber = mrn,
                        HeartRateBpm = _random.Next(70, 135),
                        OxygenSaturationPercent = _random.Next(88, 99),
                        SystolicBp = _random.Next(110, 150),
                        DiastolicBp = _random.Next(70, 95),
                        Timestamp = DateTime.UtcNow
                    };
                    await _hubContext.Clients.All.ReceiveVitalsUpdate(telemetry);
                    if (telemetry.IsCriticalAlert)
                    {
                        await _hubContext.Clients.All.ReceiveCriticalAlert(telemetry);
                    }
                }
                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}
