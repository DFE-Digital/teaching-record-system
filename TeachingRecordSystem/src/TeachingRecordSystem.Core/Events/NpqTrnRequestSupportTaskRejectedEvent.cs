namespace TeachingRecordSystem.Core.Events;

public record NpqTrnRequestSupportTaskRejectedEvent : SupportTaskUpdatedEvent
{
    public required EventModels.TrnRequestMetadata RequestData { get; init; }
    public string? RejectionReason { get; init; }
}

