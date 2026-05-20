using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Events.Models;

public record Induction
{
    public required InductionStatus Status { get; init; }
    public required InductionStatus StatusWithoutExemption { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
    public required Guid[] ExemptionReasonIds { get; init; }
    public required Option<DateTime> CpdCpdModifiedOn { get; init; }
    public required bool InductionExemptWithoutReason { get; init; }

    public static Induction FromModel(Person person) => new()
    {
        Status = person.InductionStatus,
        StatusWithoutExemption = person.InductionStatusWithoutExemption,
        StartDate = person.InductionStartDate,
        CompletedDate = person.InductionCompletedDate,
        ExemptionReasonIds = person.InductionExemptionReasonIds,
        CpdCpdModifiedOn = person.CpdInductionCpdModifiedOn.ToOption(),
        InductionExemptWithoutReason = person.InductionExemptWithoutReason
    };
}
