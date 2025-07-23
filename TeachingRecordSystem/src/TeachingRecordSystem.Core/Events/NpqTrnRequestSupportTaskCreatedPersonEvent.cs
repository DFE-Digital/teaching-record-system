namespace TeachingRecordSystem.Core.Events;

public record NpqTrnRequestSupportTaskCreatedPersonEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.SupportTask SupportTask { get; init; }
    public required EventModels.SupportTask OldSupportTask { get; init; }
    public required EventModels.TrnRequestMetadata RequestData { get; init; }
    public required EventModels.PersonDetails PersonDetails { get; init; }
    public required string? Comments { get; init; }
}

