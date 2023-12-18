using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record UserUpdatedEvent : EventBase
{
    public required User User { get; init; }
    public required UserUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum UserUpdatedEventChanges
{
    None = 0,
    Name = 1 << 0,
    Roles = 1 << 1
}
