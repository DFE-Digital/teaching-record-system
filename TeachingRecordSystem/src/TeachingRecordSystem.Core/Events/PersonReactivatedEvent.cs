namespace TeachingRecordSystem.Core.Events;

public record PersonReactivatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    public required Guid PersonId { get; init; }
    public required string? Reason { get; init; }
    public required string? ReasonDetail { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
}
