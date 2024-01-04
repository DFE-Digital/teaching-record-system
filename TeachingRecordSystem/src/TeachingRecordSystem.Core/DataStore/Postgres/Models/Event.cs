using System.Text.Json;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Event
{
    public required Guid EventId { get; init; }
    public required string EventName { get; init; }
    public required DateTime Created { get; init; }
    public required DateTime Inserted { get; init; }
    public required string Payload { get; init; }
    public bool Published { get; set; }

    public static Event FromEventBase(EventBase @event, DateTime? inserted)
    {
        var eventName = @event.GetEventName();
        var payload = JsonSerializer.Serialize(@event, inputType: @event.GetType(), EventBase.JsonSerializerOptions);

        return new Event()
        {
            EventId = @event.EventId,
            EventName = eventName,
            Created = @event.CreatedUtc,
            Inserted = inserted ?? @event.CreatedUtc,
            Payload = payload
        };
    }

    public EventBase ToEventBase()
    {
        var eventTypeName = $"{typeof(EventBase).Namespace}.{EventName}";
        var eventType = Type.GetType(eventTypeName) ??
            throw new Exception($"Could not find event type '{eventTypeName}'.");

        return (EventBase)JsonSerializer.Deserialize(Payload, eventType, EventBase.JsonSerializerOptions)!;
    }
}
