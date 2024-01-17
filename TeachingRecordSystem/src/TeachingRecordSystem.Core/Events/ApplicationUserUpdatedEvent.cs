using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record class ApplicationUserUpdatedEvent : EventBase
{
    public required ApplicationUser ApplicationUser { get; init; }
    public required ApplicationUser OldApplicationUser { get; init; }
    public required ApplicationUserUpdatedEventChanges Changes { get; init; }
}

public enum ApplicationUserUpdatedEventChanges
{
    None = 0,
    Name = 1 << 0,
    ApiRoles = 1 << 1,
}
