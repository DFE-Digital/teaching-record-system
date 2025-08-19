namespace TeachingRecordSystem.Core.Events;

public record ChangeNameRequestSupportTaskRejectedEvent : SupportTaskUpdatedEvent, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.ChangeNameRequestData RequestData { get; init; }
    public required string? RejectionReason { get; init; }
}
