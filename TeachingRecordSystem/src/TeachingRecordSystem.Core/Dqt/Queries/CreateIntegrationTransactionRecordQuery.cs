namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateIntegrationTransactionRecordQuery : ICrmQuery<Guid>
{
    public required Guid IntegrationTransactionId { get; init; }
    public required string Reference { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid? InitialTeacherTrainingId { get; init; }
    public required Guid? QualificationId { get; init; }
    public required Guid? InductionId { get; init; }
    public required Guid? InductionPeriodId { get; init; }
    public required bool? Duplicate { get; init; }
}
