namespace TeachingRecordSystem.Core.Events.Legacy;

public record PersonStatusUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required PersonStatus Status { get; init; }
    public required PersonStatus OldStatus { get; init; }
    public required string? Reason { get; init; }
    public required string? ReasonDetail { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
    public required DateOnly? DateOfDeath { get; init; }
}
