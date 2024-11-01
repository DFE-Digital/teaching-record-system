namespace TeachingRecordSystem.Core.Dqt.Queries;
public record UpdateInductionPeriodQuery : ICrmQuery<bool>
{
    public required Guid? InductionPeriodID { get; init; }
    public required Guid? InductionID { get; init; }
    public required Guid? AppropriateBodyID { get; init; }
    public required DateTime? InductionStartDate { get; init; }
    public required DateTime? InductionEndDate { get; init; }
}
