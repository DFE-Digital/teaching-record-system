namespace TeachingRecordSystem.Core.Events;

public record UserActivatedEvent : EventBase
{
    public required User User { get; init; }
}
