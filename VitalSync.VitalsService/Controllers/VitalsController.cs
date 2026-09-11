using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VitalSync.VitalsService.DTOs;
using VitalSync.VitalsService.Hubs;

namespace VitalSync.VitalsService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VitalsController : ControllerBase
    {
        private readonly IHubContext<VitalsHub, IVitalsClient> _hubContext;
        private readonly ILogger<VitalsController> _logger;
        public VitalsController(IHubContext<VitalsHub, IVitalsClient> hubContext, ILogger<VitalsController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }
        /// <summary>
        /// Ingests live telemetry from bedside monitors and broadcasts to SignalR clients.
        /// </summary>
        [HttpPost("telemetry")]
        public async Task<IActionResult> IngestTelemetry([FromBody] VitalsTelemetryDto telemetry)
        {
            if (telemetry.PatientId == Guid.Empty)
            {
                return BadRequest(new { Message = "PatientId is required." });
            }
            telemetry.Timestamp = DateTime.UtcNow;
            // Broadcast to connected Nurse Dashboards via SignalR
            await _hubContext.Clients.All.ReceiveVitalsUpdate(telemetry);
            if (telemetry.IsCriticalAlert)
            {
                _logger.LogWarning("CRITICAL ALERT triggered for Patient {PatientId}!", telemetry.PatientId);
                await _hubContext.Clients.All.ReceiveCriticalAlert(telemetry);
            }
            return Ok(new { Message = "Telemetry ingested successfully.", telemetry.IsCriticalAlert });
        }
    }
}
