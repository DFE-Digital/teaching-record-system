namespace TeachingRecordSystem.Core.Events;

public record PersonCreatedEvent : EventBase, IEventWithPersonId, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required EventModels.PersonAttributes PersonAttributes { get; init; }
    public EventModels.PersonAttributes? OldPersonAttributes => null;
    public required string? CreateReason { get; init; }
    public required string? CreateReasonDetail { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
}
