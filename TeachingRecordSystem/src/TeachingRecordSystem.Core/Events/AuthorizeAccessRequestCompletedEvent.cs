namespace TeachingRecordSystem.Core.Events;

public record AuthorizeAccessRequestCompletedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [];
}
