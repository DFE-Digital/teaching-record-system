namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateInductionPeriodTransactionalQuery : ICrmTransactionalQuery<Guid>
{
    public required Guid Id { get; init; }
    public required Guid InductionId { get; init; }
    public required Guid? AppropriateBodyId { get; init; }
    public required DateTime? InductionStartDate { get; init; }
    public required DateTime? InductionEndDate { get; init; }
}
