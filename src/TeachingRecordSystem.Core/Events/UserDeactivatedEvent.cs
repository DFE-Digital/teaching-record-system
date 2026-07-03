namespace TeachingRecordSystem.Core.Events;

public record UserDeactivatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.User User { get; init; }
    public string? DeactivatedReason { get; init; }
    public string? DeactivatedReasonDetail { get; init; }
    public Guid? EvidenceFileId { get; init; }
}
