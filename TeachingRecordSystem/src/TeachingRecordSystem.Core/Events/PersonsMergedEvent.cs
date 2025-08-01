using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record PersonsMergedEvent : EventBase, IEventWithPersonAttributes, IEventWithSecondaryPersonId
{
    public required Guid PersonId { get; init; }
    public required string PersonTrn { get; init; }
    public required Guid SecondaryPersonId { get; init; }
    public required string SecondaryPersonTrn { get; init; }
    public required PersonStatus SecondaryPersonStatus { get; init; }
    public required PersonAttributes PersonAttributes { get; init; }
    public required PersonAttributes OldPersonAttributes { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
    public required string? Comments { get; init; }
    public required PersonsMergedEventChanges Changes { get; init; }
}

[Flags]
public enum PersonsMergedEventChanges
{
    None = 0,
    FirstName = PersonAttributesChanges.FirstName,
    MiddleName = PersonAttributesChanges.MiddleName,
    LastName = PersonAttributesChanges.LastName,
    DateOfBirth = PersonAttributesChanges.DateOfBirth,
    EmailAddress = PersonAttributesChanges.EmailAddress,
    NationalInsuranceNumber = PersonAttributesChanges.NationalInsuranceNumber,
    Gender = PersonAttributesChanges.Gender
}
