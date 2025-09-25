using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace TeachingRecordSystem.Core.Models.SupportTasks;

public interface ISupportTaskData
{
    public static JsonSerializerOptions SerializerOptions => new()
    {
        TypeInfoResolver = new SupportTaskDataTypeResolver()
    };

    private class SupportTaskDataTypeResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var jsonTypeInfo = base.GetTypeInfo(type, options);

            if (type == typeof(ISupportTaskData))
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions()
                {
                    TypeDiscriminatorPropertyName = "$support-task-type",
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
                };

                foreach (var supportTaskType in SupportTaskTypeRegistry.GetAll())
                {
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(
                        new JsonDerivedType(supportTaskType.DataType, (int)supportTaskType.SupportTaskType));
                }
            }

            return jsonTypeInfo;
        }
    }
}
