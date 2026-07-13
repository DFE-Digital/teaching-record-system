namespace TeachingRecordSystem.Core.Events;

public record SupportTaskNoteCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [SupportTaskNote.SupportTaskReference];
    public required EventModels.SupportTaskNote SupportTaskNote { get; init; }
}
