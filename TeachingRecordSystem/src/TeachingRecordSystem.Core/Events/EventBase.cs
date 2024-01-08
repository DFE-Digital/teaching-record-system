using System.Text.Json;
using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public abstract record EventBase
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new();

    public required Guid EventId { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public required RaisedByUserInfo RaisedBy { get; init; }

    public string GetEventName() => GetType().Name;

    public string Serialize() => JsonSerializer.Serialize(this, inputType: GetType(), JsonSerializerOptions);

    public static EventBase Deserialize(string payload, string eventName)
    {
        var eventTypeName = $"{typeof(EventBase).Namespace}.{eventName}";
        var eventType = Type.GetType(eventTypeName) ??
            throw new Exception($"Could not find event type '{eventTypeName}'.");

        return (EventBase)JsonSerializer.Deserialize(payload, eventType, JsonSerializerOptions)!;
    }
}
