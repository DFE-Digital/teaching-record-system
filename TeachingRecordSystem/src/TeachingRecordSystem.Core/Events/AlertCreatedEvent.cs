namespace TeachingRecordSystem.Core.Events;

public class AlertCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
    public required EventModels.Alert Alert { get; init; }
    public required string? Reason { get; init; }
    public required string? ReasonDetails { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
}
