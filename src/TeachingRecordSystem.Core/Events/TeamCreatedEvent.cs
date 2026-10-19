namespace TeachingRecordSystem.Core.Events;

public record TeamCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid TeamId { get; init; }
    public required string Name { get; init; }
}
