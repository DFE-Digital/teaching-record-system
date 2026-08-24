namespace TeachingRecordSystem.Core.Events;

public record DqtInductionUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.DqtInduction Induction { get; init; }
    public required EventModels.DqtInduction OldInduction { get; init; }
    public required DqtInductionUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum DqtInductionUpdatedEventChanges
{
    None = 0,
    StartDate = 1 << 0,
    CompletionDate = 1 << 2,
    Status = 1 << 3,
    ExemptionReason = 1 << 4
}
