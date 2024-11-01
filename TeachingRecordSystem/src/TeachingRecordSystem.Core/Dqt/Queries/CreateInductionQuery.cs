namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateInductionQuery : ICrmQuery<Guid>
{
    public required Guid? PersonId { get; init; }
    public required DateTime? StartDate { get; init; }
    public required DateTime? CompletionDate { get; init; }
    public required dfeta_InductionStatus? InductionStatus { get; init; }
}


