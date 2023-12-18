using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events;

public record MandatoryQualificationDeletedEvent : EventBase, IEventWithPersonId, IEventWithMandatoryQualification
{
    public required Guid PersonId { get; init; }
    public required MandatoryQualification MandatoryQualification { get; init; }
    public required string? DeletionReason { get; init; }
    public required string? DeletionReasonDetail { get; init; }
    public required File? EvidenceFile { get; init; }
}
