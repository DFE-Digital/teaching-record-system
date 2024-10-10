namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateInductionPeriodQuery : ICrmTransactionalQuery<bool>
{
    public required Guid Id { get; init; }
    public required Guid? AppropriateBodyId { get; init; }
    public required DateTime? InductionStartDate { get; init; }
    public required DateTime? InductionEndDate { get; init; }
}
