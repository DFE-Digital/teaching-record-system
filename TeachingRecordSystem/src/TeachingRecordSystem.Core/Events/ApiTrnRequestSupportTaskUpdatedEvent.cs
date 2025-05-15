namespace TeachingRecordSystem.Core.Events;

public record ApiTrnRequestSupportTaskUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.SupportTask SupportTask { get; init; }
    public required EventModels.SupportTask OldSupportTask { get; init; }
    public required EventModels.TrnRequestMetadata RequestData { get; init; }
    public required ApiTrnRequestSupportTaskUpdatedEventChanges Changes { get; init; }
    public required EventModels.TrnRequestPersonAttributes PersonAttributes { get; init; }
    public required EventModels.TrnRequestPersonAttributes? OldPersonAttributes { get; init; }
}

[Flags]
public enum ApiTrnRequestSupportTaskUpdatedEventChanges
{
    None = 0,
    Status = 1 << 0,
    PersonFirstName = 1 << 1,
    PersonMiddleName = 1 << 2,
    PersonLastName = 1 << 3,
    PersonDateOfBirth = 1 << 4,
    PersonEmailAddress = 1 << 5,
    PersonNationalInsuranceNumber = 1 << 6
}
