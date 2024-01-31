using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess;

[method: JsonConstructor]
public class SignInJourneyState(string redirectUri, AuthenticationProperties? authenticationProperties)
{
    public const string JourneyName = "SignInJourney";

    public static JourneyDescriptor JourneyDescriptor { get; } =
        new JourneyDescriptor(JourneyName, typeof(SignInJourneyState), requestDataKeys: [], appendUniqueKey: true);

    public string RedirectUri { get; } = redirectUri;

    [JsonConverter(typeof(AuthenticationTicketJsonConverter))]
    public AuthenticationTicket? AuthenticationTicket { get; set; }

    [JsonConverter(typeof(AuthenticationTicketJsonConverter))]
    public AuthenticationTicket? OneLoginAuthenticationTicket { get; set; }

    public string[][]? VerifiedNames { get; set; }

    public DateOnly[]? VerifiedDatesOfBirth { get; set; }

    public AuthenticationProperties? AuthenticationProperties { get; } = authenticationProperties;

    public void Reset()
    {
        AuthenticationTicket = null;
        OneLoginAuthenticationTicket = null;
        VerifiedNames = null;
        VerifiedDatesOfBirth = null;
    }
}

public class AuthenticationTicketJsonConverter : JsonConverter<AuthenticationTicket>
{
    private readonly TicketSerializer _ticketSerializer = TicketSerializer.Default;

    public override AuthenticationTicket? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var bytes = reader.GetBytesFromBase64();
            return _ticketSerializer.Deserialize(bytes);
        }

        throw new JsonException($"Unknown TokenType: '{reader.TokenType}'.");
    }

    public override void Write(Utf8JsonWriter writer, AuthenticationTicket value, JsonSerializerOptions options)
    {
        var bytes = _ticketSerializer.Serialize(value);
        writer.WriteBase64StringValue(bytes);
    }
}
