namespace TeachingRecordSystem.Core.Events;

public record PersonDeactivatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => IEvent.CoalescePersonIds(MergedWithPersonId, PersonId);
    string[] IEvent.OneLoginUserSubjects => [];
    public required PersonDeactivatedEventChanges Changes { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid? MergedWithPersonId { get; init; }
    public required DateOnly? DateOfDeath { get; init; }
}

[Flags]
public enum PersonDeactivatedEventChanges
{
    None = 0,
    PersonStatus = 1 << 0,
    DateOfDeath = 1 << 1,
    MergedWithPersonId = 1 << 2
}
