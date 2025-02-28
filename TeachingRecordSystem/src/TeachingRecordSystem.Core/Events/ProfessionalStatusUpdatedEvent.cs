using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events;

public record ProfessionalStatusUpdatedEvent : EventBase, IEventWithPersonId, IEventWithProfessionalStatus, IEventWithKey
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required ProfessionalStatus ProfessionalStatus { get; init; }
    public required ProfessionalStatus OldProfessionalStatus { get; init; }
    public required string? ChangeReason { get; init; }
    public required string? ChangeReasonDetail { get; init; }
    public required File? EvidenceFile { get; init; }
    public required ProfessionalStatusUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum ProfessionalStatusUpdatedEventChanges
{
    None = 0,
    ProfessionalStatus = 1 << 0, // CML TODO needed?
    Route = 1 << 1,
    Status = 1 << 2,
    StartDate = 1 << 3,
    EndDate = 1 << 4,
    AwardedDate = 1 << 5,
    TrainingSubjectIds = 1 << 6,
    TrainingAgeSpecialismType = 1 << 7,
    TrainingAgeSpecialismRangeFrom = 1 << 8,
    TrainingAgeSpecialismRangeTo = 1 << 9,
    TrainingCountry = 1 << 10,
    TrainingProvider = 1 << 11,
    InductionExemptionReason = 1 << 12
}
