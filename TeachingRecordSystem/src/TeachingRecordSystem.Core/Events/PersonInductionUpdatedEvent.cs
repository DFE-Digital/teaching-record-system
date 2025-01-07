using Optional;

namespace TeachingRecordSystem.Core.Events;

public record PersonInductionUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required InductionStatus InductionStatus { get; init; }
    public required DateOnly? InductionStartDate { get; init; }
    public required DateOnly? InductionCompletedDate { get; init; }
    public required Guid[] InductionExemptionReasonIds { get; init; }
    public required Option<InductionStatus> CpdInductionStatus { get; init; }
    public required Option<DateOnly?> CpdInductionStartDate { get; init; }
    public required Option<DateOnly?> CpdInductionCompletedDate { get; init; }
    public required Option<DateTime> CpdInductionCpdModifiedOn { get; init; }
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
    CpdInductionStatus = 1 << 4,
    CpdInductionStartDate = 1 << 5,
    CpdInductionCompletedDate = 1 << 6
}
