namespace TeachingRecordSystem.Core.Events;

public record SupportTaskCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => SupportTask.PersonId is Guid personId ? [personId] : [];
    public required EventModels.SupportTask SupportTask { get; init; }
}
