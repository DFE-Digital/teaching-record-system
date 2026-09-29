namespace TeachingRecordSystem.Core.Events;

public record PersonInductionUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.Induction Induction { get; init; }
    public required EventModels.Induction OldInduction { get; init; }
    public required PersonInductionUpdatedEventChanges Changes { get; init; }

    public static PersonInductionUpdatedEventChanges GetChanges(
        EventModels.Induction induction,
        EventModels.Induction oldInduction) =>
        PersonInductionUpdatedEventChanges.None |
        (induction.Status != oldInduction.Status ? PersonInductionUpdatedEventChanges.InductionStatus : 0) |
        (induction.StatusWithoutExemption != oldInduction.StatusWithoutExemption ? PersonInductionUpdatedEventChanges.InductionStatusWithoutExemption : 0) |
        (induction.StartDate != oldInduction.StartDate ? PersonInductionUpdatedEventChanges.InductionStartDate : 0) |
        (induction.CompletedDate != oldInduction.CompletedDate ? PersonInductionUpdatedEventChanges.InductionCompletedDate : 0) |
        (!induction.ExemptionReasonIds.ToHashSet().SetEquals(oldInduction.ExemptionReasonIds) ? PersonInductionUpdatedEventChanges.InductionExemptionReasons : 0) |
        (induction.InductionExemptWithoutReason != oldInduction.InductionExemptWithoutReason ? PersonInductionUpdatedEventChanges.InductionExemptWithoutReason : 0);
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
    InductionExemptWithoutReason = 1 << 8
}
