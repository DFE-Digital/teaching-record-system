namespace TeachingRecordSystem.Core.Events;

public record OneLoginUserCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => IEvent.CoalescePersonIds(OneLoginUser.PersonId);
    public required EventModels.OneLoginUser OneLoginUser { get; init; }
}
