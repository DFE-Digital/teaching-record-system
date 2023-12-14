using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record UserAddedEvent : EventBase
{
    public required User User { get; init; }
}
