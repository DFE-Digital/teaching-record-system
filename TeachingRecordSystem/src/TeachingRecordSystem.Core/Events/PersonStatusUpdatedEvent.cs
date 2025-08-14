namespace TeachingRecordSystem.Core.Events;

public record PersonStatusUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required PersonStatus Status { get; init; }
    public required PersonStatus OldStatus { get; init; }
    public required string? Reason { get; init; }
    public required string? ReasonDetail { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
}
