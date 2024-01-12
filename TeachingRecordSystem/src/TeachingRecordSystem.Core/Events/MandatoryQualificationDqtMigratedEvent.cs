using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record MandatoryQualificationDqtMigratedEvent : EventBase, IEventWithPersonId, IEventWithMandatoryQualification
{
    public required Guid PersonId { get; init; }
    public required MandatoryQualification MandatoryQualification { get; init; }
}
