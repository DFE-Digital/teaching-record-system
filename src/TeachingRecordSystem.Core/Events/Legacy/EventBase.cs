using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Optional.Unsafe;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.Core.Infrastructure.Json;

namespace TeachingRecordSystem.Core.Events.Legacy;

public abstract record EventBase
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        AllowOutOfOrderMetadataProperties = true,  // jsonb columns may have properties in any order
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
                Modifiers.OptionProperties,
                Modifiers.SupportTaskData
            }
        }
    };

    public required Guid EventId { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public required RaisedByUserInfo RaisedBy { get; init; }

    public string GetEventName() => GetType().Name;

    public string Serialize() => JsonSerializer.Serialize(this, inputType: GetType(), JsonSerializerOptions);

    public static IReadOnlyCollection<string> GetEventNamesForBaseType(Type type) =>
        typeof(EventBase).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true } && t.IsAssignableTo(type) && t.Namespace == typeof(EventBase).Namespace)
            .Select(t => t.Name)
            .AsReadOnly();

    public static EventBase Deserialize(string payload, string eventName)
    {
        var eventTypeName = $"{typeof(EventBase).Namespace}.{eventName}";
        var eventType = Type.GetType(eventTypeName) ??
            throw new Exception($"Could not find event type '{eventTypeName}'.");

        return (EventBase)JsonSerializer.Deserialize(payload, eventType, JsonSerializerOptions)!;
    }

    public bool TryGetPersonId(out Guid personId)
    {
        if (this is IEventWithPersonId { PersonId: var eventPersonId })
        {
            personId = eventPersonId;
            return true;
        }

        if (this is IEventWithOptionalPersonId { PersonId: { HasValue: true } eventOptionalPersonId })
        {
            personId = eventOptionalPersonId.ValueOrFailure();
            return true;
        }

        personId = Guid.Empty;
        return false;
    }
}
