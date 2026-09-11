using Microsoft.AspNetCore.Mvc;
using VitalSync.LabService.Domain.Entities;
using VitalSync.LabService.Domain.Repositories;

namespace VitalSync.LabService.Api.Controllers
{
    public record ReserveLabSlotRequest(Guid TriageId, Guid PatientId, string EquipmentType);
    public record ReleaseLabSlotRequest(Guid ReservationId, string Reason);

    [ApiController]
    [Route("api/v1/[controller]")]
    public class LabsController : ControllerBase
    {
        private readonly ILabRepository _labRepository;
        private readonly ILogger<LabsController> _logger;

        public LabsController(ILabRepository labRepository, ILogger<LabsController> logger)
        {
            _labRepository = labRepository;
            _logger = logger;
        }

        [HttpPost("reserve-slot")]
        public async Task<IActionResult> ReserveSlot([FromBody] ReserveLabSlotRequest request, CancellationToken cancellationToken)
        {
            var equipment = await _labRepository.GetAvailableEquipmentByTypeAsync(request.EquipmentType, cancellationToken);
            if (equipment == null)
            {
                return NotFound(new { Message = $"No operational lab equipment found for type '{request.EquipmentType}'." });
            }

            var slot = new LabSlot(equipment.Id, request.TriageId, request.PatientId, DateTime.UtcNow.AddMinutes(10));
            await _labRepository.AddSlotAsync(slot, cancellationToken);
            await _labRepository.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                ReservationId = slot.Id,
                EquipmentId = equipment.Id,
                EquipmentName = equipment.EquipmentName,
                EquipmentType = equipment.EquipmentType,
                RoomNumber = equipment.RoomNumber,
                ScheduledTime = slot.ScheduledTime,
                Status = slot.Status
            });
        }

        [HttpPost("release-slot")]
        public async Task<IActionResult> ReleaseSlot([FromBody] ReleaseLabSlotRequest request, CancellationToken cancellationToken)
        {
            var slot = await _labRepository.GetSlotByIdAsync(request.ReservationId, cancellationToken);
            if (slot == null)
            {
                return NotFound(new { Message = $"Lab reservation '{request.ReservationId}' not found." });
            }

            slot.ReleaseSlot(request.Reason);
            await _labRepository.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = $"Lab slot '{request.ReservationId}' released successfully." });
        }
    }
}
