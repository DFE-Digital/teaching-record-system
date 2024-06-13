using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record PersonEmploymentUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required PersonEmployment PersonEmployment { get; init; }
    public required PersonEmployment OldPersonEmployment { get; init; }
    public required PersonEmploymentUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum PersonEmploymentUpdatedEventChanges
{
    None = 0,
    StartDate = 1 << 0,
    EndDate = 1 << 1,
    EmploymentType = 1 << 2,
    EstablishmentId = 1 << 3,
    LastKnownEmployedDate = 1 << 4,
    LastExtractDate = 1 << 5,
    NationalInsuranceNumber = 1 << 6,
    PersonPostcode = 1 << 7
}
