namespace VitalSync.VitalsService.DTOs
{
    public class VitalsTelemetryDto
    {
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public int HeartRateBpm { get; set; }
        public int OxygenSaturationPercent { get; set; }
        public int SystolicBp { get; set; }
        public int DiastolicBp { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Critical Alert Trigger
        public bool IsCriticalAlert => OxygenSaturationPercent < 90 || HeartRateBpm > 130 || HeartRateBpm < 45;
    }
}
