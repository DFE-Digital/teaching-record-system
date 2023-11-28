using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public const string AuthenticationScheme = "ApiKey";
    private const string AuthenticationHeaderScheme = "Bearer";

    private readonly IApiClientRepository _clientRepository;

    public ApiKeyAuthenticationHandler(
        IApiClientRepository clientRepository,
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _clientRepository = clientRepository;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? authorizationHeader = Request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!authorizationHeader.StartsWith(AuthenticationHeaderScheme + " ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var key = authorizationHeader.Substring(AuthenticationHeaderScheme.Length + 1);
        var client = _clientRepository.GetClientByKey(key);

        if (client is null)
        {
            return Task.FromResult(AuthenticateResult.Fail($"No client found with specified API key."));
        }

        var principal = CreatePrincipal(client.ClientId, client.Roles!);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        LogContext.PushProperty("ClientId", client.ClientId);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public static ClaimsPrincipal CreatePrincipal(string clientId, IEnumerable<string> roles)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, clientId)
        });

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        return new ClaimsPrincipal(identity);
    }
}
