using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record ApiKeyCreatedEvent : EventBase, IEventWithApiKey
{
    public required ApiKey ApiKey { get; init; }
}
