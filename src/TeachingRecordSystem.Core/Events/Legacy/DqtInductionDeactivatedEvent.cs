using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record DqtInductionDeactivatedEvent : EventBase, IEventWithPersonId
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required DqtInduction Induction { get; init; }
}
