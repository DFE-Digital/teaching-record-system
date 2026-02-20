namespace TeachingRecordSystem.Core.Events;

public record PersonDeactivatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => IEvent.CoalescePersonIds(MergedWithPersonId, PersonId);
    string[] IEvent.OneLoginUserSubjects => [];
    public required Guid PersonId { get; init; }
    public required Guid? MergedWithPersonId { get; init; }
}
