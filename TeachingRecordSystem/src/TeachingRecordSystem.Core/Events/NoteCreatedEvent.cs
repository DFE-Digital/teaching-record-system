namespace TeachingRecordSystem.Core.Events;

public record NoteCreatedEvent : EventBase, IEventWithPersonId
{
    public required EventModels.Note Note { get; init; }
    public Guid PersonId => Note.PersonId;
}
