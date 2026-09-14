using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VitalSync.SagaOrchestrator.Application.DTOs;
using VitalSync.SagaOrchestrator.Application.Services;

namespace VitalSync.SagaOrchestrator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SagaController : ControllerBase
    {
        private readonly IEmergencyReservationSagaManager _sagaManager;

        public SagaController(IEmergencyReservationSagaManager sagaManager)
        {
            _sagaManager = sagaManager;
        }

        /// <summary>
        /// Triggers the Emergency Multi-Resource Reservation Saga across DoctorService and LabService.
        /// </summary>
        [HttpPost("emergency-reservation")]
        public async Task<IActionResult> TriggerEmergencyReservation([FromBody] EmergencyReservationSagaDto dto, CancellationToken cancellationToken)
        {
            var result = await _sagaManager.ExecuteSagaAsync(dto, cancellationToken);

            if (result.Status == "RolledBack" || result.Status == "Failed")
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves state and execution logs for a specific Saga instance.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSagaState(Guid id, CancellationToken cancellationToken)
        {
            var state = await _sagaManager.GetSagaStateAsync(id, cancellationToken);
            if (state == null)
            {
                return NotFound(new { Message = $"Saga [{id}] not found." });
            }

            return Ok(state);
        }
    }
}
