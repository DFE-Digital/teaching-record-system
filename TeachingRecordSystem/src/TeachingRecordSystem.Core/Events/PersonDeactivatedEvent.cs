namespace TeachingRecordSystem.Core.Events;

public record PersonDeactivatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => MergedWithPersonId is Guid mergedWithPersonId ? [mergedWithPersonId, PersonId] : [PersonId];
    public required Guid PersonId { get; init; }
    public required Guid? MergedWithPersonId { get; init; }
}
