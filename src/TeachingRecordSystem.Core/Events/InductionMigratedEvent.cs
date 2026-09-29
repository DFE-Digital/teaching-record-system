namespace TeachingRecordSystem.Core.Events;

public record InductionMigratedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required DateOnly? InductionStartDate { get; init; }
    public required DateOnly? InductionCompletedDate { get; init; }
    public required InductionStatus InductionStatus { get; init; }
    public required Guid? InductionExemptionReasonId { get; init; }
    public required EventModels.DqtInduction? DqtInduction { get; init; }
    public required string DqtInductionStatus { get; init; }
}
