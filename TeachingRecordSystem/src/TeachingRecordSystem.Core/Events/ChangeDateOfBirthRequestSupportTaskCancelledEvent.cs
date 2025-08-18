namespace TeachingRecordSystem.Core.Events;

public record ChangeDateOfBirthRequestSupportTaskCancelledEvent : SupportTaskUpdatedEvent
{
    public required EventModels.ChangeDateOfBirthRequestData RequestData { get; init; }
}
