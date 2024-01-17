using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record class ApplicationUserCreatedEvent : EventBase
{
    public required ApplicationUser ApplicationUser { get; init; }
}
