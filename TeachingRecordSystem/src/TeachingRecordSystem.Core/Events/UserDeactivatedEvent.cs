namespace TeachingRecordSystem.Core.Events;

public record UserDeactivatedEvent : EventBase
{
    public required User User { get; init; }

    public required Guid DeactivatedByUserId { get; init; }

    public required UserDeactivatedEventChanges Changes { get; init; }
}

[Flags]
public enum UserDeactivatedEventChanges
{
    Deactivated = 0
}
