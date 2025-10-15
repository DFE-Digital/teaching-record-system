namespace TeachingRecordSystem.Core.Events.Legacy;

public record PersonDetailsUpdatedEvent : EventBase, IEventWithPersonId, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required EventModels.PersonDetails PersonAttributes { get; init; }
    public required EventModels.PersonDetails OldPersonAttributes { get; init; }
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
