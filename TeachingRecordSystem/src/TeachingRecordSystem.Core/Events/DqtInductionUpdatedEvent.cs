using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record DqtInductionUpdatedEvent : EventBase, IEventWithPersonId, IEventWithKey
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required DqtInduction Induction { get; init; }
    public required DqtInduction OldInduction { get; init; }
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
