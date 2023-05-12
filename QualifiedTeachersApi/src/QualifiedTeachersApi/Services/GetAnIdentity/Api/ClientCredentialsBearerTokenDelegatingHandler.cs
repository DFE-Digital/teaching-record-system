using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace QualifiedTeachersApi.Services.GetAnIdentityApi;

public class ClientCredentialsBearerTokenDelegatingHandler : DelegatingHandler
{
    private readonly HttpClient _tokenHttpClient;
    private readonly GetAnIdentityOptions _options;
    private readonly IClock _clock;
    private string? _accessToken;
    private DateTime _expiryTime;

    public ClientCredentialsBearerTokenDelegatingHandler(
        HttpClient tokenHttpClient,
        IOptions<GetAnIdentityOptions> options,
        IClock clock)
    {
        _tokenHttpClient = tokenHttpClient;
        _options = options.Value;
        _clock = clock;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await EnsureToken();

        request.SetBearerToken(_accessToken!);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task EnsureToken()
    {
        if (_accessToken != null && _expiryTime > DateTime.UtcNow)
        {
            return;
        }

        var tokenResponse = await _tokenHttpClient.RequestClientCredentialsTokenAsync(new()
        {
            Address = _options.TokenEndpoint,
            ClientId = _options.ClientId,
            ClientSecret = _options.ClientSecret,
            Scope = "user:write user:read"
        });

        _accessToken = tokenResponse.AccessToken;
        _expiryTime = _clock.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
    }
}
