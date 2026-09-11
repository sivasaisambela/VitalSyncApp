using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VitalSync.PatientService.Application.DTOs;
using VitalSync.PatientService.Application.Services;

namespace VitalSync.PatientService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriageController : ControllerBase
    {
        private readonly ITriageAppService _triageAppService;
        public TriageController(ITriageAppService triageAppService)
        {
            _triageAppService = triageAppService;
        }
        /// <summary>
        /// Retrieves a triage record by ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TriageRecordDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var record = await _triageAppService.GetTriageRecordByIdAsync(id, cancellationToken);
            if (record == null)
            {
                return NotFound(new { Message = $"Triage record '{id}' not found." });
            }
            return Ok(record);
        }
        /// <summary>
        /// Retrieves all triage records for a specific patient.
        /// </summary>
        [HttpGet("patient/{patientId:guid}")]
        public async Task<ActionResult<IEnumerable<TriageRecordDto>>> GetByPatientId(Guid patientId, CancellationToken cancellationToken)
        {
            var records = await _triageAppService.GetTriageRecordsForPatientAsync(patientId, cancellationToken);
            return Ok(records);
        }
        /// <summary>
        /// Submits a new triage assessment for a patient.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TriageRecordDto>> CreateAssessment([FromBody] CreateTriageDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var record = await _triageAppService.CreateTriageAssessmentAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// Updates the status of an existing triage assessment (e.g. Pending -> InProgress -> Completed).
        /// </summary>
        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status, CancellationToken cancellationToken)
        {
            var success = await _triageAppService.UpdateTriageStatusAsync(id, status, cancellationToken);
            if (!success)
            {
                return NotFound(new { Message = $"Triage record '{id}' not found." });
            }
            return NoContent(); // 204 No Content for successful updates without response body
        }
    }
}
