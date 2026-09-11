using Microsoft.AspNetCore.Mvc;
using VitalSync.DoctorService.Domain.Entities;
using VitalSync.DoctorService.Domain.Repositories;

namespace VitalSync.DoctorService.Api.Controllers
{
    public record ReserveDoctorSlotRequest(Guid TriageId, Guid PatientId, string Specialty);
    public record ReleaseDoctorSlotRequest(Guid ReservationId, string Reason);

    [ApiController]
    [Route("api/v1/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorRepository doctorRepository, ILogger<DoctorsController> logger)
        {
            _doctorRepository = doctorRepository;
            _logger = logger;
        }

        [HttpPost("reserve-slot")]
        public async Task<IActionResult> ReserveSlot([FromBody] ReserveDoctorSlotRequest request, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetAvailableDoctorBySpecialtyAsync(request.Specialty, cancellationToken);
            if (doctor == null)
            {
                return NotFound(new { Message = $"No available doctor found for specialty '{request.Specialty}'." });
            }

            var slot = new DoctorSlot(doctor.Id, request.TriageId, request.PatientId, DateTime.UtcNow.AddMinutes(15));
            await _doctorRepository.AddSlotAsync(slot, cancellationToken);
            await _doctorRepository.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                ReservationId = slot.Id,
                DoctorId = doctor.Id,
                DoctorName = doctor.FullName,
                Specialty = doctor.Specialty,
                ScheduledTime = slot.ScheduledTime,
                Status = slot.Status
            });
        }

        [HttpPost("release-slot")]
        public async Task<IActionResult> ReleaseSlot([FromBody] ReleaseDoctorSlotRequest request, CancellationToken cancellationToken)
        {
            var slot = await _doctorRepository.GetSlotByIdAsync(request.ReservationId, cancellationToken);
            if (slot == null)
            {
                return NotFound(new { Message = $"Reservation '{request.ReservationId}' not found." });
            }

            slot.ReleaseSlot(request.Reason);
            await _doctorRepository.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = $"Doctor slot '{request.ReservationId}' released successfully." });
        }
    }
}
