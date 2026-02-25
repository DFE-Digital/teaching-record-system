using IdentityModel.Client;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Core.Services.GetAnIdentity.Api;

public class ClientCredentialsBearerTokenDelegatingHandler : DelegatingHandler
{
    private readonly HttpClient _tokenHttpClient;
    private readonly GetAnIdentityOptions _options;
    private readonly TimeProvider _timeProvider;
    private string? _accessToken;
    private DateTime _expiryTime;

    public ClientCredentialsBearerTokenDelegatingHandler(
        HttpClient tokenHttpClient,
        IOptions<GetAnIdentityOptions> optionsAccessor,
        TimeProvider timeProvider)
    {
        _tokenHttpClient = tokenHttpClient;
        _options = optionsAccessor.Value;
        _timeProvider = timeProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await EnsureTokenAsync();

        request.SetBearerToken(_accessToken!);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task EnsureTokenAsync()
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
            Scope = "user:write user:read trn_token:write"
        });

        _accessToken = tokenResponse.AccessToken;
        _expiryTime = _timeProvider.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
    }
}
