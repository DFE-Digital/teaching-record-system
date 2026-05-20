using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.InductionExemptions;

public record ExemptionReasonsResponse
{
    public required IEnumerable<RouteWithExemption>? RoutesWithInductionExemptions { get; init; }
    public required Dictionary<ExemptionReasonCategory, IEnumerable<InductionExemptionReason>> ExemptionReasonCategories { get; init; }
}
