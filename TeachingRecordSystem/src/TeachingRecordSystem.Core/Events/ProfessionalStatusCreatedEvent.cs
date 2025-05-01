using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record ProfessionalStatusCreatedEvent : EventBase, IEventWithPersonId, IEventWithProfessionalStatus, IEventWithKey
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required ProfessionalStatus ProfessionalStatus { get; init; }
}
