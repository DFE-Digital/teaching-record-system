namespace TeachingRecordSystem.Core.Events.Models;

public record DqtInduction
{
    public required Guid InductionId { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletionDate { get; init; }
    public required string? InductionStatus { get; init; }
    public required string? InductionExemptionReason { get; init; }
}
