namespace TeachingRecordSystem.Core.Events;

public record ChangeDateOfBirthRequestSupportTaskRejectedEvent : SupportTaskUpdatedEvent
{
    public required EventModels.ChangeDateOfBirthRequestData RequestData { get; init; }
    public required string? RejectionReason { get; init; }
}
