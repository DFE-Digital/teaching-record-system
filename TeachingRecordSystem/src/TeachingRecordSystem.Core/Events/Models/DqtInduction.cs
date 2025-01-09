using Optional;

namespace TeachingRecordSystem.Core.Events.Models;

public record DqtInduction
{
    public required Guid InductionId { get; init; }
    public required Option<DateOnly?> StartDate { get; init; }
    public required Option<DateOnly?> CompletionDate { get; init; }
    public required Option<string?> InductionStatus { get; init; }
    public required Option<string?> InductionExemptionReason { get; init; }
}
