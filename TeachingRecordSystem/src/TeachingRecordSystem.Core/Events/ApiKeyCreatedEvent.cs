using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record ApiKeyCreatedEvent : EventBase, IEventWithApiKey
{
    public required ApiKey ApiKey { get; init; }
}
