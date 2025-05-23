namespace TeachingRecordSystem.Core.Events;

public record PersonDetailsUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.PersonDetails Details { get; init; }
    public required EventModels.PersonDetails OldDetails { get; init; }
    public required string? ChangeReason { get; init; }
    public required string? ChangeReasonDetail { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
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
    AnyNameChange = FirstName | MiddleName | LastName
}
