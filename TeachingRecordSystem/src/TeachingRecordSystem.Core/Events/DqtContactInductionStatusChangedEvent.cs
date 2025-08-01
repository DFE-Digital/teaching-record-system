namespace TeachingRecordSystem.Core.Events;

public record DqtContactInductionStatusChangedEvent : EventBase, IEventWithPersonId
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required string? InductionStatus { get; init; }
    public required string? OldInductionStatus { get; init; }
}
