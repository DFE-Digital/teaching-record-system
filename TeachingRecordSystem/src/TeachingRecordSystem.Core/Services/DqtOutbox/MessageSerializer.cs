using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core.Services.DqtOutbox;

public class MessageSerializer
{
    private static readonly string _messagesNamespace = $"{typeof(MessageSerializer).Namespace}.Messages.";

    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public dfeta_TrsOutboxMessage CreateCrmOutboxMessage(object message)
    {
        var payload = SerializeMessage(message, out var messageName);

        return new dfeta_TrsOutboxMessage()
        {
            dfeta_MessageName = messageName,
            dfeta_Payload = payload
        };
    }

    public string SerializeMessage(object message, out string messageName)
    {
        if (!message.GetType().FullName!.StartsWith(_messagesNamespace))
        {
            throw new ArgumentException("Message is not in the expected namespace.");
        }

        messageName = message.GetType().Name;
        return JsonSerializer.Serialize(message, _serializerOptions);
    }

    public object DeserializeMessage(string payload, string messageName)
    {
        var messageTypeName = $"{_messagesNamespace}{messageName}";
        var messageType = typeof(MessageSerializer).Assembly.GetType(messageTypeName) ??
            throw new ArgumentException("Could not find message type.", nameof(messageName));
        return JsonSerializer.Deserialize(payload, messageType, _serializerOptions)!;
    }
}
