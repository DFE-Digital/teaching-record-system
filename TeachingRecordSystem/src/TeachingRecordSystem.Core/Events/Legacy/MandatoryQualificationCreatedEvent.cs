using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record MandatoryQualificationCreatedEvent : EventBase, IEventWithPersonId, IEventWithMandatoryQualification
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required MandatoryQualification MandatoryQualification { get; init; }
}
