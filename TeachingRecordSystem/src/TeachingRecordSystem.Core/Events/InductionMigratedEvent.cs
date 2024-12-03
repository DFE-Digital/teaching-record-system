using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record InductionMigratedEvent : EventBase, IEventWithPersonId, IEventWithKey
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required DateOnly? InductionStartDate { get; init; }
    public required DateOnly? InductionCompletedDate { get; init; }
    public required string? InductionStatus { get; init; }
    public required string? InductionExemptionReason { get; init; }
    public required DqtInduction DqtInduction { get; init; }
}
