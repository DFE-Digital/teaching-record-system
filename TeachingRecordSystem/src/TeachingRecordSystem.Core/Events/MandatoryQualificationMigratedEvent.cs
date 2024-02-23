using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record MandatoryQualificationMigratedEvent : EventBase, IEventWithPersonId, IEventWithMandatoryQualification, IEventWithKey
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required MandatoryQualification MandatoryQualification { get; init; }
    public required MandatoryQualificationMigratedEventChanges Changes { get; init; }
}

public enum MandatoryQualificationMigratedEventChanges
{
    None = 0,
    Provider = 1 << 0,
    Specialism = 1 << 1,
}
