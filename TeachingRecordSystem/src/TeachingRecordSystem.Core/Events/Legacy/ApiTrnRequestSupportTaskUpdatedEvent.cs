namespace TeachingRecordSystem.Core.Events.Legacy;

public record ApiTrnRequestSupportTaskUpdatedEvent : SupportTaskUpdatedEvent, IEventWithPersonId, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required EventModels.TrnRequestMetadata RequestData { get; init; }
    public required ApiTrnRequestSupportTaskUpdatedEventChanges Changes { get; init; }
    public required EventModels.PersonDetails PersonAttributes { get; init; }
    public required EventModels.PersonDetails? OldPersonAttributes { get; init; }
    public required string? Comments { get; init; }
}

[Flags]
public enum ApiTrnRequestSupportTaskUpdatedEventChanges
{
    None = 0,
    Status = 1 << 0,
    PersonFirstName = PersonAttributesChanges.FirstName,
    PersonMiddleName = PersonAttributesChanges.MiddleName,
    PersonLastName = PersonAttributesChanges.LastName,
    PersonDateOfBirth = PersonAttributesChanges.DateOfBirth,
    PersonEmailAddress = PersonAttributesChanges.EmailAddress,
    PersonNationalInsuranceNumber = PersonAttributesChanges.NationalInsuranceNumber,
    PersonGender = PersonAttributesChanges.Gender,
    PersonNameChange = PersonFirstName | PersonMiddleName | PersonLastName,
    AllChanges = PersonNameChange | PersonDateOfBirth | PersonEmailAddress | PersonNationalInsuranceNumber | PersonGender
}
