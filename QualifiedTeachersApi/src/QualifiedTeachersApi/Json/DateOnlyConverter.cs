using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using QualifiedTeachersApi.ModelBinding;

namespace QualifiedTeachersApi.Json
{
    public class DateOnlyConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new InvalidOperationException($"Cannot get the value of a token type '{reader.TokenType}' as a string.");
            }

            var asString = reader.GetString();
            if (!DateOnly.TryParseExact(asString, Constants.DateFormat, out var value))
            {
                throw new JsonException("The JSON value is not in a supported DateOnly format.");
            }

            return value;
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            var asString = value.ToString(Constants.DateFormat);
            writer.WriteStringValue(asString);
        }
    }
}
