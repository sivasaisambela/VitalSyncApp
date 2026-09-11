using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VitalSync.PatientService.Application.DTOs;
using VitalSync.PatientService.Application.Services;

namespace VitalSync.PatientService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientAppService _patientAppService;
        public PatientsController(IPatientAppService patientAppService)
        {
            _patientAppService = patientAppService;
        }

        /// <summary>
        /// Retrieves all registered patients.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll(CancellationToken cancellationToken)
        {
            var patients = await _patientAppService.GetAllPatientsAsync(cancellationToken);
            return Ok(patients);
        }

        /// <summary>
        /// Retrieves a patient by Guid ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PatientDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var patient = await _patientAppService.GetPatientByIdAsync(id, cancellationToken);
            if (patient == null)
            {
                return NotFound(new { Message = $"Patient with ID '{id}' was not found." });
            }
            return Ok(patient);
        }

        /// <summary>
        /// Retrieves a patient by Medical Record Number (MRN).
        /// </summary>
        [HttpGet("mrn/{mrn}")]
        public async Task<ActionResult<PatientDto>> GetByMrn(string mrn, CancellationToken cancellationToken)
        {
            var patient = await _patientAppService.GetPatientByMrnAsync(mrn, cancellationToken);
            if (patient == null)
            {
                return NotFound(new { Message = $"Patient with MRN '{mrn}' was not found." });
            }
            return Ok(patient);
        }
        /// <summary>
        /// Registers a new patient.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PatientDto>> Register([FromBody] CreatePatientDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdPatient = await _patientAppService.RegisterPatientAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = createdPatient.Id }, createdPatient);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
