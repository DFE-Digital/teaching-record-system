namespace TeachingRecordSystem.Core.Events;

public record ChangeNameRequestSupportTaskCancelledEvent : SupportTaskUpdatedEvent
{
    public required EventModels.ChangeNameRequestData RequestData { get; init; }
}
