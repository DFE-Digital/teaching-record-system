using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events;

public record ProfessionalStatusUpdatedEvent : EventBase, IEventWithPersonId, IEventWithProfessionalStatus
{
    public required Guid PersonId { get; init; }
    public required RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
    public required RouteToProfessionalStatus OldRouteToProfessionalStatus { get; init; }
    public required ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
    public required string? ChangeReason { get; init; }
    public required string? ChangeReasonDetail { get; init; }
    public required File? EvidenceFile { get; init; }
    public required ProfessionalStatusUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum ProfessionalStatusUpdatedEventChanges
{
    None = 0,
    Route = 1 << 0,
    Status = 1 << 1,
    StartDate = 1 << 2,
    EndDate = 1 << 3,
    AwardedDate = 1 << 4,
    TrainingSubjectIds = 1 << 5,
    TrainingAgeSpecialismType = 1 << 6,
    TrainingAgeSpecialismRangeFrom = 1 << 7,
    TrainingAgeSpecialismRangeTo = 1 << 8,
    TrainingCountry = 1 << 9,
    TrainingProvider = 1 << 10,
    InductionExemptionReasons = 1 << 11,
    DegreeType = 1 << 12,
    PersonQtsDate = 1 << 13,
    PersonEytsDate = 1 << 14,
    PersonHasEyps = 1 << 15,
    PersonPqtsDate = 1 << 16
}
