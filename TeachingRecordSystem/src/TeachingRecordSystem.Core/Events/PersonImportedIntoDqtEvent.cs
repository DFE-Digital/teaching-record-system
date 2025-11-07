namespace TeachingRecordSystem.Core.Events;

public record PersonImportedIntoDqtEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
    public required EventModels.DqtPersonDetails Details { get; init; }
}
