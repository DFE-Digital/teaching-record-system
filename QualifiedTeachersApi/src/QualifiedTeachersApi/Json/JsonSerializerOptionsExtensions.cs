#nullable disable
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using QualifiedTeachersApi.V3;

namespace QualifiedTeachersApi.Json;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions Configure(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new DateOnlyConverter());

        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers =
            {
                ConfigureConditionallySerializedProperties
            }
        };

        return options;

        static void ConfigureConditionallySerializedProperties(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }

            if (typeInfo.Type.IsAssignableTo(typeof(IConditionallySerializedProperties)))
            {
                foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
                {
                    propertyInfo.ShouldSerialize = (obj, prop) =>
                        ((IConditionallySerializedProperties)obj).ShouldSerializeProperty(
                            ((PropertyInfo)propertyInfo.AttributeProvider).Name);
                }
            }
        }
    }
}
