namespace TeachingRecordSystem.Core.Events;

public record PersonReactivatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required PersonReactivatedEventChanges Changes { get; init; }
    public required Guid PersonId { get; init; }
}

[Flags]
public enum PersonReactivatedEventChanges
{
    None = 0,
    PersonStatus = 1 << 0,
    DateOfDeath = 1 << 1
}
