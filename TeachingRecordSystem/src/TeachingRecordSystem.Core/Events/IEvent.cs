using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TeachingRecordSystem.Core.Infrastructure.Json;

namespace TeachingRecordSystem.Core.Events;

public interface IEvent
{
    Guid EventId { get; }
    Guid[] PersonIds { get; }

    static JsonSerializerOptions SerializerOptions => new()
    {
        AllowOutOfOrderMetadataProperties = true,  // jsonb columns may have properties in any order
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { Modifiers.Events } }
    };
}
