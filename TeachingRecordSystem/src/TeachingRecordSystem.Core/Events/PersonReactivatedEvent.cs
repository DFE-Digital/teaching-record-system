namespace TeachingRecordSystem.Core.Events;

public record PersonReactivatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
}
