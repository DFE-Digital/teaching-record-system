namespace TeachingRecordSystem.Core.Events;

public record TrnRequestCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds { get; } = [];
    string[] IEvent.OneLoginUserSubjects => [];
    public required EventModels.TrnRequestMetadata TrnRequest { get; init; }
}
