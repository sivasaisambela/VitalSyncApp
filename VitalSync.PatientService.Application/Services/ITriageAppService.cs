using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Application.DTOs;

namespace VitalSync.PatientService.Application.Services
{
    public interface ITriageAppService
    {
        Task<TriageRecordDto?> GetTriageRecordByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TriageRecordDto>> GetTriageRecordsForPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task<TriageRecordDto> CreateTriageAssessmentAsync(CreateTriageDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdateTriageStatusAsync(Guid triageId, string newStatus, CancellationToken cancellationToken = default);
    }
}
