using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record TpsEmploymentCreatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required TpsEmployment TpsEmployment { get; init; }
}
