using System.Text.Json;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Api.Endpoints.IdentityWebHooks.Messages;

namespace TeachingRecordSystem.Api.Endpoints.IdentityWebHooks;

public class NotificationEnvelopeConverter : JsonConverter<NotificationEnvelope>
{
    public override NotificationEnvelope? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        Guid? notificationId = null;
        DateTime? timeUtc = null;
        string? messageType = null;
        INotificationMessage? message = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (!notificationId.HasValue)
                {
                    throw new JsonException($"Missing required property {nameof(NotificationEnvelope.NotificationId)}");
                }

                if (!timeUtc.HasValue)
                {
                    throw new JsonException($"Missing required property {nameof(NotificationEnvelope.TimeUtc)}");
                }

                if (string.IsNullOrEmpty(messageType))
                {
                    throw new JsonException($"Missing required property {nameof(NotificationEnvelope.MessageType)}");
                }

                if (message is null)
                {
                    throw new JsonException($"Missing required property {nameof(NotificationEnvelope.Message)}");
                }

                return new NotificationEnvelope()
                {
                    NotificationId = notificationId.Value,
                    TimeUtc = timeUtc.Value,
                    MessageType = messageType!,
                    Message = message!
                };
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }

                reader.Read();

                var caseInsensitive = options.PropertyNameCaseInsensitive;

                if (propertyName.Equals(nameof(NotificationEnvelope.NotificationId), caseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
                {
                    if (reader.TryGetGuid(out var validValue))
                    {
                        notificationId = validValue;
                    }
                    else
                    {
                        throw new JsonException($"Value for {nameof(NotificationEnvelope.NotificationId)} cannot be deserialized to a {typeof(Guid).FullName}");
                    }
                }
                else if (propertyName.Equals(nameof(NotificationEnvelope.TimeUtc), caseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
                {
                    if (reader.TryGetDateTime(out var validValue))
                    {
                        timeUtc = validValue;
                    }
                    else
                    {
                        throw new JsonException($"Value for {nameof(NotificationEnvelope.TimeUtc)} cannot be deserialized to a {typeof(DateTime).FullName}");
                    }
                }
                else if (propertyName.Equals(nameof(NotificationEnvelope.MessageType), caseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
                {
                    messageType = reader.GetString();
                }
                else if (propertyName.Equals(nameof(NotificationEnvelope.Message), caseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
                {
                    if (string.IsNullOrEmpty(messageType))
                    {
                        throw new JsonException($"Cannot deserialize {nameof(NotificationEnvelope.Message)} property with missing {nameof(NotificationEnvelope.MessageType)}");
                    }

                    switch (messageType)
                    {
                        case UserCreatedMessage.MessageTypeName:
                            message = JsonSerializer.Deserialize<UserCreatedMessage>(ref reader, options);
                            break;
                        case UserUpdatedMessage.MessageTypeName:
                            message = JsonSerializer.Deserialize<UserUpdatedMessage>(ref reader, options);
                            break;
                        case UserMergedMessage.MessageTypeName:
                            message = JsonSerializer.Deserialize<UserMergedMessage>(ref reader, options);
                            break;
                        default:
                            throw new JsonException($"{nameof(NotificationEnvelope.MessageType)} '{messageType}' is not supported");
                    }
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, NotificationEnvelope value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
