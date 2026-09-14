namespace VitalSync.SagaOrchestrator.Domain.Entities
{
    public enum SagaStatus
    {
        Pending = 0,
        DoctorReserved = 1,
        Completed = 2,
        Failed = 3,
        RolledBack = 4
    }
}
