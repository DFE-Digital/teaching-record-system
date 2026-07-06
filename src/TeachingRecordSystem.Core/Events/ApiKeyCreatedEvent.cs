namespace TeachingRecordSystem.Core.Events;

public record ApiKeyCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.ApiKey ApiKey { get; init; }
}
