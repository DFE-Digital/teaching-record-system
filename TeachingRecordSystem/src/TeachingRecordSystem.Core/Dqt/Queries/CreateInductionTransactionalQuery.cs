namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateInductionTransactionalQuery : ICrmTransactionalQuery<Guid>
{
    public required Guid Id { get; init; }
    public required Guid ContactId { get; init; }
    public required DateTime? StartDate { get; init; }
    public required DateTime? CompletionDate { get; init; }
    public required dfeta_InductionStatus InductionStatus { get; init; }
}
