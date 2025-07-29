namespace TeachingRecordSystem.Core.Events;

public interface IEventWithPersonAttributes : IEventWithPersonId
{
    EventModels.PersonAttributes PersonAttributes { get; }
    EventModels.PersonAttributes? OldPersonAttributes { get; }
}

[Flags]
public enum PersonAttributesChanges
{
    FirstName = 1 << 16,
    MiddleName = 1 << 17,
    LastName = 1 << 18,
    DateOfBirth = 1 << 19,
    EmailAddress = 1 << 20,
    NationalInsuranceNumber = 1 << 21,
    Gender = 1 << 22
}
