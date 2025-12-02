namespace TeachingRecordSystem.Core.Events;

public record TrnRequestCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds { get; } = [];
    public required EventModels.TrnRequestMetadata TrnRequest { get; init; }
}
