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
    InductionStatus = 1 << 0,
    InductionStartDate = 1 << 1,
    InductionCompletedDate = 1 << 2,
    InductionExemptionReasons = 1 << 3,
    InductionStatusWithoutExemption = 1 << 7,
}
