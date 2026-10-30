namespace TeachingRecordSystem.Core.Events;

public record TeamUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.Team Team { get; init; }
    public required EventModels.Team OldTeam { get; init; }
}
