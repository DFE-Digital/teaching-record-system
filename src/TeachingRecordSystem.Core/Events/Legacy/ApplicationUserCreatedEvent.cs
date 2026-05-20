using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record class ApplicationUserCreatedEvent : EventBase
{
    public required ApplicationUser ApplicationUser { get; init; }
}
