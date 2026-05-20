namespace TeachingRecordSystem.Core.Events;

public class NoteCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    public required Guid PersonId { get; init; }
    public required EventModels.Note Note { get; init; }
}
