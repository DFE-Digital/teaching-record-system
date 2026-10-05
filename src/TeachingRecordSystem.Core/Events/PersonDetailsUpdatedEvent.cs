namespace TeachingRecordSystem.Core.Events;

public record PersonDetailsUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.PersonDetails PersonDetails { get; init; }
    public required EventModels.PersonDetails OldPersonDetails { get; init; }
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
    OtherThanNameChange = DateOfBirth | EmailAddress | NationalInsuranceNumber | Gender
}
