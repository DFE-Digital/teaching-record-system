using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record class AlertCreatedEvent : EventBase, IEventWithPersonId, IEventWithAlert
{
    public required Guid PersonId { get; init; }
    public required Alert Alert { get; init; }
}
