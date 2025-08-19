namespace TeachingRecordSystem.Core.Events;

public record ChangeDateOfBirthRequestSupportTaskRejectedEvent : SupportTaskUpdatedEvent, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.ChangeDateOfBirthRequestData RequestData { get; init; }
    public required string? RejectionReason { get; init; }
}
