namespace TeachingRecordSystem.Core.Events;

public record ApplicationUserCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.ApplicationUser ApplicationUser { get; init; }
}
