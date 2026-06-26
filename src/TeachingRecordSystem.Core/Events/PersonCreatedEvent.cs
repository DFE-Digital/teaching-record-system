namespace TeachingRecordSystem.Core.Events;

public record PersonCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.PersonDetails Details { get; init; }
    public required EventModels.TrnRequestMetadata? TrnRequestMetadata { get; init; }
}
