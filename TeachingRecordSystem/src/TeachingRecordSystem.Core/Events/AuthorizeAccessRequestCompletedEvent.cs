namespace TeachingRecordSystem.Core.Events;

public record AuthorizeAccessRequestCompletedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
}
