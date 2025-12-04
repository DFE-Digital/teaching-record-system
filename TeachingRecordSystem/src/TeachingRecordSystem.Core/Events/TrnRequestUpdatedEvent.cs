namespace TeachingRecordSystem.Core.Events;

public record TrnRequestUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [];
    public required Guid SourceApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required TrnRequestUpdatedChanges Changes { get; init; }
    public required EventModels.TrnRequestMetadata TrnRequest { get; init; }
    public required EventModels.TrnRequestMetadata OldTrnRequest { get; init; }
    public required string? ReasonDetails { get; init; }
}

[Flags]
public enum TrnRequestUpdatedChanges
{
    None = 0,
    Status = 1 << 0,
    ResolvedPersonId = 1 << 1
}
