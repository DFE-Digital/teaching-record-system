namespace TeachingRecordSystem.Core.Events;

public record SupportTaskDeletedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => SupportTask.PersonId is Guid personId ? [personId] : [];
    public required string SupportTaskReference { get; init; }
    public required EventModels.SupportTask SupportTask { get; init; }
    public required string? ReasonDetail { get; init; }
}
