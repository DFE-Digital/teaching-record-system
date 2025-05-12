using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events;

public record class ProfessionalStatusDeletedEvent : EventBase, IEventWithPersonId, IEventWithProfessionalStatus
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required ProfessionalStatus ProfessionalStatus { get; init; }
    public required string? DeletionReason { get; init; }
    public required string? DeletionReasonDetail { get; init; }
    public required File? EvidenceFile { get; init; }
    public required ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
}
