namespace TeachingRecordSystem.Core.Events;

public record PersonCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
    public required EventModels.PersonDetails Details { get; init; }
    public required string? CreateReason { get; init; }
    public required string? CreateReasonDetail { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
    public required EventModels.TrnRequestMetadata? TrnRequestMetadata { get; init; }
}
