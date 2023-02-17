using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace QualifiedTeachersApi.Services.GetAnIdentityApi;

public class ClientCredentialsBearerTokenDelegatingHandler : DelegatingHandler
{
    private TokenResponse _accessToken { get; set; }
    private DateTime _expiryTime { get; set; }
    private GetAnIdentityApiOptions _options { get; set; }
    private IClock _clock;

    public ClientCredentialsBearerTokenDelegatingHandler(IOptions<GetAnIdentityApiOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await EnsureToken();
        request.SetBearerToken(_accessToken.AccessToken);
        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }

    private async Task EnsureToken()
    {
        if (_accessToken != null && _expiryTime > DateTime.UtcNow)
        {
            return;
        }

        var tokenClient = new HttpClient();
        _accessToken = await tokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = _options.TokenEndpoint,
            ClientId = _options.ClientId,
            ClientSecret = _options.ClientSecret,
            Scope = "user:write user:read"
        });
        _expiryTime = _clock.UtcNow.AddSeconds(_accessToken.ExpiresIn);
    }
}
