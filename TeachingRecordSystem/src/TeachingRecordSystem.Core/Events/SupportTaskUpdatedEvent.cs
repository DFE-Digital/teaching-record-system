namespace TeachingRecordSystem.Core.Events;

public record SupportTaskUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => SupportTask.PersonId is Guid personId ? [personId] : [];
    public required string SupportTaskReference { get; init; }
    public required SupportTaskUpdatedEventChanges Changes { get; init; }
    public required EventModels.SupportTask SupportTask { get; init; }
    public required EventModels.SupportTask OldSupportTask { get; init; }
    public required string? Comments { get; init; }
}

[Flags]
public enum SupportTaskUpdatedEventChanges
{
    None = 0,
    Status = 1 << 0,
    Data = 1 << 1
}
