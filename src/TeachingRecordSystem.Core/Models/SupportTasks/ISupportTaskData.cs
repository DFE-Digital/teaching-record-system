using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TeachingRecordSystem.Core.Infrastructure.Json;

namespace TeachingRecordSystem.Core.Models.SupportTasks;

public interface ISupportTaskData
{
    static JsonSerializerOptions SerializerOptions => new()
    {
        AllowOutOfOrderMetadataProperties = true,  // jsonb columns may have properties in any order
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { Modifiers.SupportTaskData } }
    };
}
