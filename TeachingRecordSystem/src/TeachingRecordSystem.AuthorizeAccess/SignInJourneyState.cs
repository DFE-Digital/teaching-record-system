using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;

namespace TeachingRecordSystem.AuthorizeAccess;

public class SignInJourneyState
{
    public const string JourneyName = "SignInJourney";

    private readonly TicketSerializer _ticketSerializer = TicketSerializer.Default;

    [JsonInclude]
    private byte[]? _oneLoginAuthenticationTicket;

    [JsonConstructor]
    public SignInJourneyState(string redirectUri, string oneLoginAuthenticationScheme)
    {
        RedirectUri = redirectUri;
        OneLoginAuthenticationScheme = oneLoginAuthenticationScheme;
    }

    public AuthenticationTicket? AuthenticationTicket { get; private set; }

    [JsonIgnore]
    public AuthenticationTicket? OneLoginAuthenticationTicket =>
        _oneLoginAuthenticationTicket is not null ? _ticketSerializer.Deserialize(_oneLoginAuthenticationTicket) : null;

    [JsonIgnore]
    public bool AuthenticatedWithOneLogin => OneLoginAuthenticationTicket is not null;

    public string RedirectUri { get; }

    public string OneLoginAuthenticationScheme { get; }

    public void OnSignedInWithOneLogin(AuthenticationTicket ticket)
    {
        _oneLoginAuthenticationTicket = _ticketSerializer.Serialize(ticket);
        // TODO Should we reset all other state here?
    }

    public void Reset()
    {
        AuthenticationTicket = null;
        _oneLoginAuthenticationTicket = null;
    }
}
