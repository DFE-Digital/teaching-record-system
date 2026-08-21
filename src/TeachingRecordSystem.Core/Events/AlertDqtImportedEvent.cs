namespace TeachingRecordSystem.Core.Events;

public record AlertDqtImportedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.Alert Alert { get; init; }
    public required int DqtState { get; init; }
}
