namespace TeachingRecordSystem.Core.Events;

public record PersonMigratedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required string? Trn { get; init; }
    public required EventModels.PersonDetails PersonAttributes { get; init; }
}
