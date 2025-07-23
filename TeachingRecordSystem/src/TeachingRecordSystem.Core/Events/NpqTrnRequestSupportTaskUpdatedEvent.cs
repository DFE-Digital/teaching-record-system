namespace TeachingRecordSystem.Core.Events;

public record NpqTrnRequestSupportTaskUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.SupportTask SupportTask { get; init; }
    public required EventModels.SupportTask OldSupportTask { get; init; }
    public required EventModels.TrnRequestMetadata RequestData { get; init; }
    public required NpqTrnRequestSupportTaskUpdatedEventChanges Changes { get; init; }
    public required EventModels.TrnRequestPersonAttributes PersonAttributes { get; init; }
    public required EventModels.TrnRequestPersonAttributes? OldPersonAttributes { get; init; }
    public required string? Comments { get; init; }
}

[Flags]
public enum NpqTrnRequestSupportTaskUpdatedEventChanges
{
    None = 0,
    Status = 1 << 0,
    PersonDateOfBirth = 1 << 4,
    PersonEmailAddress = 1 << 5,
    PersonNationalInsuranceNumber = 1 << 6,
}

