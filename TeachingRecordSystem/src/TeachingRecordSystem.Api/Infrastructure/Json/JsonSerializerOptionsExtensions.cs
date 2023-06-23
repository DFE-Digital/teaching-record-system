using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace TeachingRecordSystem.Api.Infrastructure.Json;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions AddConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    internal static JsonSerializerOptions Configure(this JsonSerializerOptions options)
    {
        options.AddConverters();

        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers =
            {
                Modifiers.OptionProperties
            }
        };

        return options;
    }
}
