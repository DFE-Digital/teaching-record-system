using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.Core.Infrastructure.Json;

namespace TeachingRecordSystem.Core.Events;

public abstract record EventBase
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers =
            {
                // Remove any required constraints;
                // we want to be able to evolve the events by adding new properties with the required modifier
                // without breaking deserialization for older events.
                static typeInfo =>
                {
                    if (typeInfo.Kind != JsonTypeInfoKind.Object)
                    {
                        return;
                    }

                    foreach (var propertyInfo in typeInfo.Properties)
                    {
                        propertyInfo.IsRequired = false;
                    }
                },
                Modifiers.OptionProperties
            }
        }
    };

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
