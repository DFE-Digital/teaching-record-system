using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record DqtInductionReactivatedEvent : EventBase, IEventWithPersonId
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required DqtInduction Induction { get; init; }
}
