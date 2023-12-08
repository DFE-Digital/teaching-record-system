using System.Text.Json;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core;

/// <inheritdoc cref="EventInfo"/>
public sealed class EventInfo<TEvent> : EventInfo
    where TEvent : EventBase
{
    public EventInfo(TEvent @event)
        : base(@event)
    {
    }

    public static implicit operator TEvent(EventInfo<TEvent> @event) => @event.Event;

    public new TEvent Event => (TEvent)base.Event;
}

/// <summary>
/// Wrapper type for serializing an event to JSON.
/// </summary>
public abstract class EventInfo
{
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
    {
        Converters =
        {
            new EventInfoJsonConverter()
        }
    };

    private protected EventInfo(EventBase @event)
    {
        Event = @event;
    }

    public EventBase Event { get; }

    public static EventInfo<TEvent> Create<TEvent>(TEvent @event) where TEvent : EventBase => new EventInfo<TEvent>(@event);

    public static EventInfo Deserialize(string payload) =>
        JsonSerializer.Deserialize<EventInfo>(payload, _serializerOptions)!;

    public static EventInfo<TEvent> Deserialize<TEvent>(string payload) where TEvent : EventBase =>
        Deserialize(payload) is EventInfo<TEvent> typedEvent ? typedEvent :
            throw new InvalidCastException();

    public string Serialize() => JsonSerializer.Serialize(this, typeof(EventInfo), _serializerOptions);
}

file class EventInfoJsonConverter : JsonConverter<EventInfo>
{
    public override EventInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        Type? eventType = null;

        while (reader.Read())
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = reader.GetString();

            if (propertyName == "EventTypeName")
            {
                reader.Read();

                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException();
                }

                var eventTypeName = typeof(EventBase).Namespace + "." + reader.GetString()!;
                eventType = Type.GetType(eventTypeName);
            }
            else if (propertyName == "Event")
            {
                reader.Read();

                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                if (eventType is null)
                {
                    throw new JsonException();
                }

                var @event = JsonSerializer.Deserialize(ref reader, eventType, EventBase.JsonSerializerOptions);

                reader.Read();

                var eventInfoType = typeof(EventInfo<>).MakeGenericType(eventType);
                return (EventInfo)Activator.CreateInstance(eventInfoType, [@event])!;
            }
        }

        throw new Exception();
    }

    public override void Write(Utf8JsonWriter writer, EventInfo value, JsonSerializerOptions options)
    {
        var eventType = value.Event.GetType();
        var eventTypeName = eventType.Name;

        writer.WriteStartObject();
        writer.WritePropertyName("EventTypeName");
        writer.WriteStringValue(eventTypeName);
        writer.WritePropertyName("Event");
        JsonSerializer.Serialize(writer, value.Event, eventType, EventBase.JsonSerializerOptions);
        writer.WriteEndObject();
    }
}
