namespace TeachingRecordSystem.Core.Events;

public record UserAddedEvent : EventBase
{
    public required User User { get; init; }
    public required Guid AddedByUserId { get; init; }
}