namespace TeachingRecordSystem.Core.Events;

public record TrnAllocatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required string Trn { get; init; }
}
