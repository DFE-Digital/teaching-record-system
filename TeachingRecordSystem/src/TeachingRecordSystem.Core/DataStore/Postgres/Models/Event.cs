using System.Text.Json;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Event
{
    private static readonly JsonSerializerOptions _serializerOptions = new();

    public long EventId { get; set; }
    public required string EventName { get; init; }
    public required DateTime Created { get; init; }
    public required string Payload { get; init; }
    public bool Published { get; set; }

    public static Event FromEventBase(EventBase @event)
    {
        var eventName = @event.GetType().Name;
        var payload = JsonSerializer.Serialize(@event, inputType: @event.GetType(), _serializerOptions);

        return new Event()
        {
            Created = @event.CreatedUtc,
            EventName = eventName,
            Payload = payload
        };
    }

    public EventBase ToEventBase()
    {
        var eventTypeName = $"{typeof(EventBase).Namespace}.{EventName}";
        var eventType = Type.GetType(eventTypeName) ??
            throw new Exception($"Could not find event type '{eventTypeName}'.");

        return (EventBase)JsonSerializer.Deserialize(Payload, eventType, _serializerOptions)!;
    }
}
