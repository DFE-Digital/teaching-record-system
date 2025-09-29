using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record AlertDqtDeactivatedEvent : EventBase, IEventWithPersonId, IEventWithAlert
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required Alert Alert { get; init; }
}
