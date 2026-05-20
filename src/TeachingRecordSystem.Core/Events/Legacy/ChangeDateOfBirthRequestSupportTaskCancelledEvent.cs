namespace TeachingRecordSystem.Core.Events.Legacy;

public record ChangeDateOfBirthRequestSupportTaskCancelledEvent : SupportTaskUpdatedEvent, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.ChangeDateOfBirthRequestData RequestData { get; init; }
}
