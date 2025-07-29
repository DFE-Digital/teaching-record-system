namespace TeachingRecordSystem.Core.Events;

public record NpqTrnRequestSupportTaskUpdatedEvent : SupportTaskUpdatedEvent, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required EventModels.TrnRequestMetadata RequestData { get; init; }
    public required NpqTrnRequestSupportTaskUpdatedEventChanges Changes { get; init; }
    public required EventModels.PersonAttributes PersonAttributes { get; init; }
    public required EventModels.PersonAttributes? OldPersonAttributes { get; init; }
    public required string? Comments { get; init; }
}

[Flags]
public enum NpqTrnRequestSupportTaskUpdatedEventChanges
{
    None = 0,
    Status = 1 << 0,
    PersonFirstName = PersonAttributesChanges.FirstName,
    PersonMiddleName = PersonAttributesChanges.MiddleName,
    PersonLastName = PersonAttributesChanges.LastName,
    PersonDateOfBirth = PersonAttributesChanges.DateOfBirth,
    PersonEmailAddress = PersonAttributesChanges.EmailAddress,
    PersonNationalInsuranceNumber = PersonAttributesChanges.NationalInsuranceNumber,
    PersonGender = PersonAttributesChanges.Gender
}

