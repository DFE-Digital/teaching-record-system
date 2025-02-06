namespace TeachingRecordSystem.Core.Events;

public record PersonInductionUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.Induction Induction { get; init; }
    public required EventModels.Induction OldInduction { get; init; }
    public required string? ChangeReason { get; init; }
    public required string? ChangeReasonDetail { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
    public required PersonInductionUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum PersonInductionUpdatedEventChanges
{
    None = 0,
    Status = 1 << 0,
    StartDate = 1 << 1,
    CompletedDate = 1 << 2,
    ExemptionReasons = 1 << 3,
    InductionStatus = 1 << 4,
    FailedInWalesStartDate = 1 << 7,
    FailedInWalesCompletedDate = 1 << 8,
    RequiredToComplete = 1 << 9,
    Passed = 1 << 10,
    Failed = 1 << 11,
    CpdStatus = 1 << 12
}
