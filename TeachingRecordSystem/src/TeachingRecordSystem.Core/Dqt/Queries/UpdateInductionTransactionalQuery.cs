namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateInductionTransactionalQuery : ICrmTransactionalQuery<bool>
{
    public required Guid InductionId { get; init; }
    public required DateTime? CompletionDate { get; init; }
    public required dfeta_InductionStatus InductionStatus { get; init; }
}
