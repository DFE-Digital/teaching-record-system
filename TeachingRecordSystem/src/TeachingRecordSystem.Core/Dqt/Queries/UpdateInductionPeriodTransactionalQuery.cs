namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateInductionPeriodTransactionalQuery : ICrmTransactionalQuery<bool>
{
    public required Guid InductionPeriodId { get; init; }
    public required Guid? AppropriateBodyId { get; init; }
    public required DateTime? InductionStartDate { get; init; }
    public required DateTime? InductionEndDate { get; init; }
}
