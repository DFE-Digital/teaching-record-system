namespace TeachingRecordSystem.Core.Events;

public class NoteImportedIntoDqtEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
    public required EventModels.Note Note { get; init; }
}
