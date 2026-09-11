using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalSync.PatientService.Application.DTOs;
using VitalSync.PatientService.Domain.Entities;
using VitalSync.PatientService.Domain.Repositories;

namespace VitalSync.PatientService.Application.Services
{
    public class PatientAppService : IPatientAppService
    {
        private readonly IPatientRepository _patientRepository;
        public PatientAppService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<PatientDto?> GetPatientByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var patient = await _patientRepository.GetByIdAsync(id, cancellationToken);
            return patient == null ? null : MapToDto(patient);
        }
        public async Task<PatientDto?> GetPatientByMrnAsync(string mrn, CancellationToken cancellationToken = default)
        {
            var patient = await _patientRepository.GetByMrnAsync(mrn, cancellationToken);
            return patient == null ? null : MapToDto(patient);
        }
        public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync(CancellationToken cancellationToken = default)
        {
            var patients = await _patientRepository.GetAllAsync(cancellationToken);
            return patients.Select(MapToDto);
        }
        public async Task<PatientDto> RegisterPatientAsync(CreatePatientDto dto, CancellationToken cancellationToken = default)
        {
            // Determine MRN: Use provided MRN if present, otherwise auto-generate
            string mrnToUse = !string.IsNullOrWhiteSpace(dto.MedicalRecordNumber)
                ? dto.MedicalRecordNumber
                : $"MRN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            // Check for duplicate MRN
            var existing = await _patientRepository.GetByMrnAsync(mrnToUse, cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException($"Patient with MRN '{mrnToUse}' already exists.");
            }

            // Construct Domain Entity
            var patient = new Patient(
                mrnToUse,
                dto.FullName,
                dto.Age,
                dto.Gender,
                dto.BloodGroup,
                dto.KnownAllergies,
                dto.MedicalHistory
            );

            await _patientRepository.AddAsync(patient, cancellationToken);
            await _patientRepository.SaveChangesAsync(cancellationToken);

            return MapToDto(patient);
        }

        // Helper method to map Entity to DTO
        private static PatientDto MapToDto(Patient patient)
        {
            return new PatientDto
            {
                Id = patient.Id,
                MedicalRecordNumber = patient.MedicalRecordNumber,
                FullName = patient.FullName,
                Age = patient.Age,
                Gender = patient.Gender,
                BloodGroup = patient.BloodGroup,
                KnownAllergies = patient.KnownAllergies,
                MedicalHistory = patient.MedicalHistory,
                CreatedAt = patient.CreatedAt,
                TriageRecords = patient.TriageRecords.Select(t => new TriageRecordDto
                {
                    Id = t.Id,
                    PatientId = t.PatientId,
                    ReportedSymptoms = t.ReportedSymptoms,
                    BloodPressure = t.BloodPressure,
                    HeartRateBpm = t.HeartRateBpm,
                    OxygenSaturationPercent = t.OxygenSaturationPercent,
                    Priority = t.Priority,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };
        }
    }
}
