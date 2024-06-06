namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateTaskQuery : ICrmQuery<Guid>
{
    public required Guid ContactId { get; init; }
    public required string Category { get; init; }
    public required string Subject { get; init; }
    public required string Description { get; init; }
    public required DateTime? ScheduledEnd { get; init; }
}
