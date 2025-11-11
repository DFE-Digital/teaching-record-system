namespace TeachingRecordSystem.Core.Events;

public record EmailSentEvent : IEvent
{
    public Guid EventId { get; set; }
    public required Guid? PersonId { get; set; }
    public Guid[] PersonIds => PersonId is Guid personId ? [personId] : [];
    public required EventModels.Email Email { get; init; }
}
