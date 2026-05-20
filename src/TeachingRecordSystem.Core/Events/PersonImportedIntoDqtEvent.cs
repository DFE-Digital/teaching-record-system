namespace TeachingRecordSystem.Core.Events;

public record PersonImportedIntoDqtEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    public required Guid PersonId { get; init; }
    public required EventModels.DqtPersonDetails Details { get; init; }
}
