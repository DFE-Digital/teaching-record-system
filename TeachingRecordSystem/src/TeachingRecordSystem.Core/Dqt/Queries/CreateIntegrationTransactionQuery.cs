namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateIntegrationTransactionQuery : ICrmQuery<Guid>
{
    public required int TypeId { get; init; }
    public required DateTime StartDate {get;init;}
}
