namespace TeachingRecordSystem.Core.Events;

public record AlertCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    public required Guid PersonId { get; init; }
    public required EventModels.Alert Alert { get; init; }
}
