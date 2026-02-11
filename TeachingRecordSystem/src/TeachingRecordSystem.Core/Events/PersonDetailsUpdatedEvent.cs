namespace TeachingRecordSystem.Core.Events;

public record PersonDetailsUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    public required Guid PersonId { get; init; }
    public required EventModels.PersonDetails PersonDetails { get; init; }
    public required EventModels.PersonDetails OldPersonDetails { get; init; }
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
    NationalInsuranceNumber = 1 << 5,
    Gender = 1 << 6,
    NameChange = FirstName | MiddleName | LastName,
    OtherThanNameChange = ~None & ~NameChange
}

public static class PersonDetailsUpdatedEventExtensions
{
    public static LegacyEvents.PersonDetailsUpdatedEventChanges ToLegacyPersonDetailsUpdatedEventChanges(this PersonDetailsUpdatedEventChanges changes) =>
        LegacyEvents.PersonDetailsUpdatedEventChanges.None
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? LegacyEvents.PersonDetailsUpdatedEventChanges.FirstName : 0)
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? LegacyEvents.PersonDetailsUpdatedEventChanges.MiddleName : 0)
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? LegacyEvents.PersonDetailsUpdatedEventChanges.LastName : 0)
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? LegacyEvents.PersonDetailsUpdatedEventChanges.DateOfBirth : 0)
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? LegacyEvents.PersonDetailsUpdatedEventChanges.EmailAddress : 0)
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? LegacyEvents.PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0)
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? LegacyEvents.PersonDetailsUpdatedEventChanges.Gender : 0);

    public static LegacyEvents.ChangeNameRequestSupportTaskApprovedEventChanges ToLegacyChangeNameRequestSupportTaskApprovedEventChanges(this PersonDetailsUpdatedEventChanges changes) =>
        LegacyEvents.ChangeNameRequestSupportTaskApprovedEventChanges.None
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? LegacyEvents.ChangeNameRequestSupportTaskApprovedEventChanges.FirstName : 0)
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? LegacyEvents.ChangeNameRequestSupportTaskApprovedEventChanges.MiddleName : 0)
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? LegacyEvents.ChangeNameRequestSupportTaskApprovedEventChanges.LastName : 0);

    public static LegacyEvents.ChangeDateOfBirthRequestSupportTaskApprovedEventChanges ToLegacyChangeDateOfBirthRequestSupportTaskApprovedEventChanges(this PersonDetailsUpdatedEventChanges changes) =>
        LegacyEvents.ChangeDateOfBirthRequestSupportTaskApprovedEventChanges.None
        | (changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? LegacyEvents.ChangeDateOfBirthRequestSupportTaskApprovedEventChanges.DateOfBirth : 0);


}
