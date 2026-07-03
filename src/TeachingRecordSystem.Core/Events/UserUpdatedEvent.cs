namespace TeachingRecordSystem.Core.Events;

public record UserUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.User User { get; init; }
    public required UserUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum UserUpdatedEventChanges
{
    None = 0,
    Name = 1 << 0,
    Roles = 1 << 1
}
