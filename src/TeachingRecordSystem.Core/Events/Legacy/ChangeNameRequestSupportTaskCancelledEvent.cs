namespace TeachingRecordSystem.Core.Events.Legacy;

public record ChangeNameRequestSupportTaskCancelledEvent : SupportTaskUpdatedEvent, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.ChangeNameRequestData RequestData { get; init; }
}
