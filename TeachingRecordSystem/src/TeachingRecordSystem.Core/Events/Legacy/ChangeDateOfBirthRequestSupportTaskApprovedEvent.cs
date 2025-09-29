namespace TeachingRecordSystem.Core.Events.Legacy;

public record ChangeDateOfBirthRequestSupportTaskApprovedEvent : SupportTaskUpdatedEvent, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required EventModels.ChangeDateOfBirthRequestData RequestData { get; init; }
    public required ChangeDateOfBirthRequestSupportTaskApprovedEventChanges Changes { get; init; }
    public required EventModels.PersonAttributes PersonAttributes { get; init; }
    public required EventModels.PersonAttributes? OldPersonAttributes { get; init; }
}

[Flags]
public enum ChangeDateOfBirthRequestSupportTaskApprovedEventChanges
{
    None = 0,
    DateOfBirth = PersonAttributesChanges.DateOfBirth
}
