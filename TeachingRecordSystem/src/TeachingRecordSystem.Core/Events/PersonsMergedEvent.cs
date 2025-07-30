namespace TeachingRecordSystem.Core.Events;

public record PersonsMergedEvent : EventBase, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required string? PrimaryRecordTrn { get; init; }
    public required string? SecondaryRecordTrn { get; init; }
    public required EventModels.PersonAttributes PersonAttributes { get; init; }
    public required EventModels.PersonAttributes OldPersonAttributes { get; init; }
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
