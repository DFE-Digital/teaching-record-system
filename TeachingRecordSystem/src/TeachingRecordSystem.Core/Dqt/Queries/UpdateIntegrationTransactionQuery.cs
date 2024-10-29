namespace TeachingRecordSystem.Core.Dqt.Queries;
public record UpdateIntegrationTransactionQuery : ICrmQuery<bool>
{
    public Guid IntegrationTransactionId { get; init; }
    public DateTime? EndDate { get; init; }
    public int? TotalCount { get; init; }
    public int? SuccessCount { get; init; }
    public int? DuplicateCount { get; init; }
    public int? FailureCount { get; init; }
    public string? FailureMessage { get; init; }
}
