using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record SupportTaskCreatedEvent : EventBase
{
    public required SupportTask SupportTask { get; init; }
}
