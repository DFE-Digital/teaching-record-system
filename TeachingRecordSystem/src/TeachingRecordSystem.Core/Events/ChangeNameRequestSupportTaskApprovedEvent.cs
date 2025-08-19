namespace TeachingRecordSystem.Core.Events;

public record ChangeNameRequestSupportTaskApprovedEvent : SupportTaskUpdatedEvent, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required EventModels.ChangeNameRequestData RequestData { get; init; }
    public required ChangeNameRequestSupportTaskApprovedEventChanges Changes { get; init; }
    public required EventModels.PersonAttributes PersonAttributes { get; init; }
    public required EventModels.PersonAttributes? OldPersonAttributes { get; init; }
}

[Flags]
public enum ChangeNameRequestSupportTaskApprovedEventChanges
{
    None = 0,
    FirstName = PersonAttributesChanges.FirstName,
    MiddleName = PersonAttributesChanges.MiddleName,
    LastName = PersonAttributesChanges.LastName,
    NameChange = FirstName | MiddleName | LastName,
}
