namespace TeachingRecordSystem.Core.Events.Legacy;

public record NpqTrnRequestSupportTaskRejectedEvent : SupportTaskUpdatedEvent
{
    public required EventModels.TrnRequestMetadata RequestData { get; init; }
    public required string? RejectionReason { get; init; }
}

