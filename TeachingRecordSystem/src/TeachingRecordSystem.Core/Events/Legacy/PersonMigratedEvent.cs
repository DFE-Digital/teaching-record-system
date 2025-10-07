namespace TeachingRecordSystem.Core.Events.Legacy;

public record PersonMigratedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required string? Trn { get; init; }
    public required EventModels.PersonAttributes PersonAttributes { get; init; }
}
