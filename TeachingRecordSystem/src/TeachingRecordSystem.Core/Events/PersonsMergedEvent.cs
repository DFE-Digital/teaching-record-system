namespace TeachingRecordSystem.Core.Events;

public record PersonsMergedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [RetainedPersonId, DeactivatedPersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    public required Guid RetainedPersonId { get; init; }
    public required string RetainedPersonTrn { get; init; }
    public required Guid DeactivatedPersonId { get; init; }
    public required string DeactivatedPersonTrn { get; init; }
    public required PersonStatus DeactivatedPersonStatus { get; init; }
    public required EventModels.PersonDetails RetainedPersonDetails { get; init; }
    public required EventModels.PersonDetails OldRetainedPersonDetails { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
    public required string? Comments { get; init; }
    public required PersonsMergedEventChanges Changes { get; init; }
}

[Flags]
public enum PersonsMergedEventChanges
{
    None = 0,
    FirstName = 1 << 0,
    MiddleName = 1 << 1,
    LastName = 1 << 2,
    DateOfBirth = 1 << 3,
    EmailAddress = 1 << 4,
    NationalInsuranceNumber = 1 << 5,
    Gender = 1 << 6,
    NameChange = FirstName | MiddleName | LastName,
    OtherThanNameChange = ~None & ~NameChange
}

public static class PersonsMergedEventChangesExtensions
{
    public static LegacyEvents.PersonsMergedEventChanges ToLegacyPersonsMergedEventChanges(this PersonsMergedEventChanges changes) =>
        LegacyEvents.PersonsMergedEventChanges.None
        | (changes.HasFlag(PersonsMergedEventChanges.FirstName) ? LegacyEvents.PersonsMergedEventChanges.FirstName : 0)
        | (changes.HasFlag(PersonsMergedEventChanges.MiddleName) ? LegacyEvents.PersonsMergedEventChanges.MiddleName : 0)
        | (changes.HasFlag(PersonsMergedEventChanges.LastName) ? LegacyEvents.PersonsMergedEventChanges.LastName : 0)
        | (changes.HasFlag(PersonsMergedEventChanges.DateOfBirth) ? LegacyEvents.PersonsMergedEventChanges.DateOfBirth : 0)
        | (changes.HasFlag(PersonsMergedEventChanges.EmailAddress) ? LegacyEvents.PersonsMergedEventChanges.EmailAddress : 0)
        | (changes.HasFlag(PersonsMergedEventChanges.NationalInsuranceNumber) ? LegacyEvents.PersonsMergedEventChanges.NationalInsuranceNumber : 0)
        | (changes.HasFlag(PersonsMergedEventChanges.Gender) ? LegacyEvents.PersonsMergedEventChanges.Gender : 0);
}
