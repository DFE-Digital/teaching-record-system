namespace TeachingRecordSystem.Core.Events;

public record PersonDetailsUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.PersonDetails Details { get; init; }
    public required EventModels.PersonDetails OldDetails { get; init; }
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
    FirstName = 1 << 0,
    MiddleName = 1 << 1,
    LastName = 1 << 2,
    DateOfBirth = 1 << 3,
    EmailAddress = 1 << 4,
    MobileNumber = 1 << 5,
    NationalInsuranceNumber = 1 << 6,
    Gender = 1 << 7,
    NameChange = FirstName | MiddleName | LastName,
    OtherThanNameChange = DateOfBirth | EmailAddress | MobileNumber | NationalInsuranceNumber | Gender
}
