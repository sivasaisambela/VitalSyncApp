using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Application.DTOs;

namespace VitalSync.PatientService.Application.Services
{
    public interface IPatientAppService
    {
        Task<PatientDto?> GetPatientByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PatientDto?> GetPatientByMrnAsync(string mrn, CancellationToken cancellationToken = default);
        Task<IEnumerable<PatientDto>> GetAllPatientsAsync(CancellationToken cancellationToken = default);
        Task<PatientDto> RegisterPatientAsync(CreatePatientDto dto, CancellationToken cancellationToken = default);
    }
}
