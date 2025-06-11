using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events;

public record RouteToProfessionalStatusUpdatedEvent : EventBase, IEventWithPersonId, IEventWithProfessionalStatus
{
    public required Guid PersonId { get; init; }
    public required RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
    public required RouteToProfessionalStatus OldRouteToProfessionalStatus { get; init; }
    public required string? ChangeReason { get; init; }
    public required string? ChangeReasonDetail { get; init; }
    public required File? EvidenceFile { get; init; }
    public required RouteToProfessionalStatusUpdatedEventChanges Changes { get; init; }
    public required ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
    public required Induction Induction { get; init; }
    public required Induction OldInduction { get; init; }
}

[Flags]
public enum RouteToProfessionalStatusUpdatedEventChanges
{
    None = 0,
    Route = 1 << 0,
    Status = 1 << 1,
    StartDate = 1 << 2,
    EndDate = 1 << 3,
    HoldsFrom = 1 << 4,
    TrainingSubjectIds = 1 << 5,
    TrainingAgeSpecialismType = 1 << 6,
    TrainingAgeSpecialismRangeFrom = 1 << 7,
    TrainingAgeSpecialismRangeTo = 1 << 8,
    TrainingCountry = 1 << 9,
    TrainingProvider = 1 << 10,
    ExemptFromInduction = 1 << 11,
    DegreeType = 1 << 12,
    ExemptFromInductionDueToQtsDate = 1 << 13,
    // Keep the following options aligned with other ProfessionalStatus events
    PersonQtlsStatus = 1 << 24,
    PersonQtsDate = 1 << 25,
    PersonEytsDate = 1 << 26,
    PersonHasEyps = 1 << 27,
    PersonPqtsDate = 1 << 28,
    PersonInductionStatus = 1 << 29,
    PersonInductionStatusWithoutExemption = 1 << 30
}
