using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record AlertMigratedEvent : EventBase, IEventWithPersonId, IEventWithAlert, IEventWithKey
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required Alert Alert { get; init; }
}
