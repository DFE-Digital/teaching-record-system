namespace TeachingRecordSystem.Core.Events;

public record UserAddedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.User User { get; init; }
}
