using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record MandatoryQualificationCreatedEvent : EventBase, IEventWithPersonId, IEventWithMandatoryQualification, IEventWithKey
{
    public required string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required MandatoryQualification MandatoryQualification { get; init; }
}
