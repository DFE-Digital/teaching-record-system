namespace TeachingRecordSystem.Core.Events.Legacy;

public record NoteCreatedEvent : EventBase, IEventWithPersonId
{
    public required EventModels.Note Note { get; init; }
    public Guid PersonId => Note.PersonId;
}
