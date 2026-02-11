using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TeachingRecordSystem.Core.Infrastructure.Json;

namespace TeachingRecordSystem.Core.Events;

public interface IEvent
{
    Guid EventId { get; }
    Guid[] PersonIds { get; }
    string[] OneLoginUserSubjects { get; }

    public static JsonSerializerOptions SerializerOptions => new()
    {
        AllowOutOfOrderMetadataProperties = true,  // jsonb columns may have properties in any order
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { Modifiers.Events, Modifiers.SupportTaskData } }
    };

    protected static Guid[] CoalescePersonIds(params IEnumerable<Guid?> personIds) =>
        personIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToArray();
}
