using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events;

public record class ProfessionalStatusDeletedEvent : EventBase, IEventWithPersonId, IEventWithProfessionalStatus
{
    public required Guid PersonId { get; init; }
    public required RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
    public required string? DeletionReason { get; init; }
    public required string? DeletionReasonDetail { get; init; }
    public required File? EvidenceFile { get; init; }
    public required ProfessionalStatusDeletedEventChanges Changes { get; init; }
    public required ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
}

[Flags]
public enum ProfessionalStatusDeletedEventChanges
{
    None = 0,
    PersonQtsDate = 1 << 0,
    PersonEytsDate = 1 << 1,
    PersonHasEyps = 1 << 2,
    PersonPqtsDate = 1 << 3
}

