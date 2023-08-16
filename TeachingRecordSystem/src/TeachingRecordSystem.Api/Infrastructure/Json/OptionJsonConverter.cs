using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Optional;
using Optional.Unsafe;

namespace TeachingRecordSystem.Api.Infrastructure.Json;

public class OptionJsonConverter<T> : JsonConverter<Option<T>>
{
    public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var asT = JsonSerializer.Deserialize<T>(ref reader, options)!;
        return Option.Some(asT);
    }

    public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options)
    {
        Debug.Assert(value.HasValue);  // Modifier should ensure we only get here if there's a value
        JsonSerializer.Serialize(writer, value.ValueOrFailure(), options);
    }
}
