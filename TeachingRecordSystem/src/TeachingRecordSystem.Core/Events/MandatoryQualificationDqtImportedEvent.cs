using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record MandatoryQualificationDqtImportedEvent : EventBase, IEventWithPersonId, IEventWithMandatoryQualification, IEventWithKey
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required MandatoryQualification MandatoryQualification { get; init; }
    public required int DqtState { get; init; }
}
