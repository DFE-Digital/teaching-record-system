namespace TeachingRecordSystem.Core.Events;

public record AuthorizeAccessRequestStartedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    public required string JourneyInstanceId { get; init; }
    public required Guid ApplicationUserId { get; init; }
    public required string ClientId { get; init; }
}
