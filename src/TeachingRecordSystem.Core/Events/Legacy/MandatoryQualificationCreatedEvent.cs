using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record MandatoryQualificationCreatedEvent : EventBase, IEventWithPersonId, IEventWithMandatoryQualification
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required MandatoryQualification MandatoryQualification { get; init; }
    public required string? AddReason { get; init; }
    public required string? AddReasonDetail { get; init; }
    public required File? EvidenceFile { get; init; }
}
