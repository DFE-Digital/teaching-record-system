using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record UserDeactivatedEvent : EventBase
{
    public required User User { get; init; }
}
