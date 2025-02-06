using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Events.Models;

public record Induction
{
    public required InductionStatus Status { get; init; }
    public required bool RequiredToComplete { get; init; }
    public required bool Passed { get; init; }
    public required bool Failed { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
    public required Guid[] ExemptionReasonIds { get; init; }
    public required Option<InductionStatus> CpdStatus { get; init; }
    public required Option<DateTime> CpdCpdModifiedOn { get; init; }
    public required DateOnly? FailedInWalesStartDate { get; init; }
    public required DateOnly? FailedInWalesCompletedDate { get; init; }

    public static Induction FromModel(Person person) => new()
    {
        Status = person.InductionStatus,
        RequiredToComplete = person.InductionRequiredToComplete,
        Passed = person.InductionPassed,
        Failed = person.InductionFailed,
        StartDate = person.InductionStartDate,
        CompletedDate = person.InductionCompletedDate,
        ExemptionReasonIds = person.InductionExemptionReasonIds,
        CpdStatus = person.CpdInductionStatus.ToOption(),
        CpdCpdModifiedOn = person.CpdInductionCpdModifiedOn.ToOption(),
        FailedInWalesStartDate = person.InductionFailedInWalesStartDate,
        FailedInWalesCompletedDate = person.InductionFailedInWalesCompletedDate
    };
}
