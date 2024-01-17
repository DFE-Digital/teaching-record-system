using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record ApiKeyUpdatedEvent : EventBase, IEventWithApiKey
{
    public required ApiKey ApiKey { get; init; }
    public required ApiKey OldApiKey { get; init; }
    public required ApiKeyUpdatedEventChanges Changes { get; init; }
}

public enum ApiKeyUpdatedEventChanges
{
    None = 0,
    Expires = 1 << 0
}
