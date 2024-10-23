namespace TeachingRecordSystem.Core.Dqt.Queries;

public record DeleteIntegrationTransactionQuery : ICrmQuery<bool>
{
    public required Guid IntegrationTransactionId { get; init; }
}

