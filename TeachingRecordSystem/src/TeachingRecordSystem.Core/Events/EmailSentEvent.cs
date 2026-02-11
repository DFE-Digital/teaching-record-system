namespace TeachingRecordSystem.Core.Events;

public record EmailSentEvent : IEvent
{
    public Guid EventId { get; set; }
    Guid[] IEvent.PersonIds => IEvent.CoalescePersonIds(PersonId);
    public required Guid? PersonId { get; set; }
    string[] IEvent.OneLoginUserSubjects => [];
    public required EventModels.Email Email { get; init; }
}
