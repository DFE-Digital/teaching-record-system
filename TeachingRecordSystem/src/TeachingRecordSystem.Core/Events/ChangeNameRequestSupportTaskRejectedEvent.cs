namespace TeachingRecordSystem.Core.Events;

public record ChangeNameRequestSupportTaskRejectedEvent : SupportTaskUpdatedEvent
{
    public required EventModels.ChangeNameRequestData RequestData { get; init; }
    public required string? RejectionReason { get; init; }
}
