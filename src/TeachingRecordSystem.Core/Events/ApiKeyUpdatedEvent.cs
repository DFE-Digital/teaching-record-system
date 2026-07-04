namespace TeachingRecordSystem.Core.Events;

public record ApiKeyUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.ApiKey ApiKey { get; init; }
    public required EventModels.ApiKey OldApiKey { get; init; }
    public required ApiKeyUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum ApiKeyUpdatedEventChanges
{
    None = 0,
    Expires = 1 << 0
}
