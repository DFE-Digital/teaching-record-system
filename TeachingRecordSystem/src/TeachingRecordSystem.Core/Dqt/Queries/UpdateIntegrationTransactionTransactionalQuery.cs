namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateIntegrationTransactionTransactionalQuery : ICrmTransactionalQuery<bool>
{
    public required Guid IntegrationTransactionId { get; init; }
    public required DateTime? EndDate { get; init; }
    public required int? TotalCount { get; init; }
    public required int? SuccessCount { get; init; }
    public required int? DuplicateCount { get; init; }
    public required int? FailureCount { get; init; }
    public required string? FailureMessage { get; init; }
}
