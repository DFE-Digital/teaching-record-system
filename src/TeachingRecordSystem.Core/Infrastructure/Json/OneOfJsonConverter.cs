using System.Text.Json;
using System.Text.Json.Serialization;
using OneOf;

namespace TeachingRecordSystem.Core.Infrastructure.Json;

public class OneOfJsonConverter<T0, T1> : JsonConverter<OneOf<T0, T1>>
{
    public override OneOf<T0, T1> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, OneOf<T0, T1> value, JsonSerializerOptions options)
    {
        value.Switch(
            v => JsonSerializer.Serialize(writer, v, options),
            v => JsonSerializer.Serialize(writer, v, options));
    }
}

public class OneOfJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(OneOf<,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(OneOfJsonConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
