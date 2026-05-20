using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record UserActivatedEvent : EventBase
{
    public required User User { get; init; }
}
