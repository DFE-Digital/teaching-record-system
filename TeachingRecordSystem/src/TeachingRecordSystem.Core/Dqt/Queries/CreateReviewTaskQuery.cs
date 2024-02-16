namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateReviewTaskQuery : ICrmQuery<Guid>
{
    public required Guid ContactId { get; init; }
    public required string Category { get; init; }
    public required string Subject { get; init; }
    public required string Description { get; init; }
}
