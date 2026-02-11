namespace TeachingRecordSystem.Core.Events;

public record NoteUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    public required Guid PersonId { get; init; }
    public required NoteUpdatedInDqtEventChanges Changes { get; init; }
    public required EventModels.Note Note { get; init; }
    public required EventModels.Note OldNote { get; init; }
}

[Flags]
public enum NoteUpdatedInDqtEventChanges
{
    None = 0,
    Content = 1 << 0,
    File = 1 << 1,
}
