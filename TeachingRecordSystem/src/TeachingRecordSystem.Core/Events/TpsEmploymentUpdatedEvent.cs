using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record TpsEmploymentUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required TpsEmployment TpsEmployment { get; init; }
    public required TpsEmployment OldTpsEmployment { get; init; }
    public required TpsEmploymentUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum TpsEmploymentUpdatedEventChanges
{
    None = 0,
    StartDate = 1 << 0,
    EndDate = 1 << 1,
    EmploymentType = 1 << 2,
    EstablishmentId = 1 << 3,
    LastKnownTpsEmployedDate = 1 << 4,
    LastExtractDate = 1 << 5,
    NationalInsuranceNumber = 1 << 6,
    PersonPostcode = 1 << 7,
    WithdrawalConfirmed = 1 << 8,
    PersonEmailAddress = 1 << 9,
    EmployerPostcode = 1 << 10,
    EmployerEmailAddress = 1 << 11
}
