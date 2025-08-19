namespace TeachingRecordSystem.Core.Events;

public record PersonDetailsUpdatedEvent : EventBase, IEventWithPersonId, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required EventModels.PersonAttributes PersonAttributes { get; init; }
    public required EventModels.PersonAttributes OldPersonAttributes { get; init; }
    public required string? NameChangeReason { get; init; }
    public required EventModels.File? NameChangeEvidenceFile { get; init; }
    public required string? DetailsChangeReason { get; init; }
    public required string? DetailsChangeReasonDetail { get; init; }
    public required EventModels.File? DetailsChangeEvidenceFile { get; init; }
    public required PersonDetailsUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum PersonDetailsUpdatedEventChanges
{
    None = 0,
    FirstName = PersonAttributesChanges.FirstName,
    MiddleName = PersonAttributesChanges.MiddleName,
    LastName = PersonAttributesChanges.LastName,
    DateOfBirth = PersonAttributesChanges.DateOfBirth,
    EmailAddress = PersonAttributesChanges.EmailAddress,
    NationalInsuranceNumber = PersonAttributesChanges.NationalInsuranceNumber,
    Gender = PersonAttributesChanges.Gender,
    NameChange = FirstName | MiddleName | LastName,
    OtherThanNameChange = DateOfBirth | EmailAddress | NationalInsuranceNumber | Gender
}
