using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record MandatoryQualificationUpdatedEvent : EventBase, IEventWithPersonId, IEventWithMandatoryQualification
{
    public required Guid PersonId { get; init; }
    public required MandatoryQualification MandatoryQualification { get; init; }
    public required MandatoryQualification OldMandatoryQualification { get; init; }
    public required MandatoryQualificationUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum MandatoryQualificationUpdatedEventChanges
{
    None = 0,
    Provider = 1 << 0,
    Specialism = 1 << 2,
    Status = 1 << 3,
    StartDate = 1 << 4,
    EndDate = 1 << 5,
}
