namespace TeachingRecordSystem.Core.Events;

public record UserDeactivatedEvent : EventBase
{
    public required User User { get; init; }

    public required Guid UpdatedByUserId { get; init; }

    public required UserActivationEventChanges Changes { get; init; }
}
