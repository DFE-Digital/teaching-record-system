namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateInductionPeriodQuery : ICrmQuery<Guid>
{
    public required Guid? InductionID { get; init; }
    public required Guid? AppropriateBodyID { get; init; }
    public required DateTime? InductionStartDate { get; init; }
    public required DateTime? InductionEndDate { get; init; }
}
