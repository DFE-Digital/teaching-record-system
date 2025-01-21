using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Events.Models;

public record Induction
{
    public required InductionStatus Status { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
    public required Guid[] ExemptionReasonIds { get; init; }
    public required Option<InductionStatus> CpdStatus { get; init; }
    public required Option<DateOnly?> CpdStartDate { get; init; }
    public required Option<DateOnly?> CpdCompletedDate { get; init; }
    public required Option<DateTime> CpdCpdModifiedOn { get; init; }

    public static Induction FromModel(Person person) => new()
    {
        Status = person.InductionStatus,
        StartDate = person.InductionStartDate,
        CompletedDate = person.InductionCompletedDate,
        ExemptionReasonIds = person.InductionExemptionReasonIds,
        CpdStatus = person.CpdInductionStatus.ToOption(),
        CpdStartDate = person.CpdInductionStatus is not null ? Option.Some(person.CpdInductionStartDate) : default,
        CpdCompletedDate = person.CpdInductionStatus is not null ? Option.Some(person.CpdInductionCompletedDate) : default,
        CpdCpdModifiedOn = person.CpdInductionCpdModifiedOn.ToOption()
    };
}
