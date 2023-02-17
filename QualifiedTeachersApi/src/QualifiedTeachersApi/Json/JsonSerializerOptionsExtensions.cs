using System.Text.Json;
using System.Text.Json.Serialization;

namespace QualifiedTeachersApi.Json
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions AddConverters(this JsonSerializerOptions options)
        {
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new DateOnlyConverter());

            return options;
        }
    }
}
