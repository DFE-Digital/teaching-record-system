namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Event
{
    public const string KeyUniqueIndexName = "ix_events_key";

    public required Guid EventId { get; init; }
    public required string EventName { get; init; }
    public required DateTime Created { get; init; }
    public required DateTime Inserted { get; init; }
    public required string Payload { get; init; }
    public string? Key { get; init; }
    public bool Published { get; set; }
    public Guid? PersonId { get; init; }

    public static Event FromEventBase(EventBase @event, DateTime? inserted)
    {
        var eventName = @event.GetEventName();
        var payload = @event.Serialize();

        return new Event()
        {
            EventId = @event.EventId,
            EventName = eventName,
            Created = @event.CreatedUtc,
            Inserted = inserted ?? @event.CreatedUtc,
            Payload = payload,
            Key = (@event as IEventWithKey)?.Key,
            PersonId = (@event as IEventWithPersonId)?.PersonId
        };
    }

    public EventBase ToEventBase()
    {
        return EventBase.Deserialize(Payload, EventName);
    }
}
