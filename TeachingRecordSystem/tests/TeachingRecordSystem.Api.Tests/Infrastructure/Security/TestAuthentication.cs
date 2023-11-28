using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.Tests.Infrastructure.Security;

public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
{
    private readonly CurrentApiClientProvider _currentApiClientProvider;

    public TestAuthenticationHandler(
        CurrentApiClientProvider currentApiClientProvider,
        IOptionsMonitor<TestAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) :
        base(options, logger, encoder, clock)
    {
        _currentApiClientProvider = currentApiClientProvider;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Use-CurrentClientIdProvider", out var useCurrentClientIdProvider) ||
            useCurrentClientIdProvider != "true")
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var currentApiClientId = _currentApiClientProvider.CurrentApiClientId;
        var currentRoles = _currentApiClientProvider.Roles!;

        if (currentApiClientId is not null)
        {
            var principal = ApiKeyAuthenticationHandler.CreatePrincipal(currentApiClientId, currentRoles);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        else
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}

public class TestAuthenticationOptions : AuthenticationSchemeOptions { }

public class CurrentApiClientProvider
{
    private readonly AsyncLocal<string> _currentApiClientId = new();
    private readonly AsyncLocal<string[]> _roles = new();

    [DisallowNull]
    public string? CurrentApiClientId
    {
        get => _currentApiClientId.Value;
        set => _currentApiClientId.Value = value;
    }

    [DisallowNull]
    public string[]? Roles
    {
        get => _roles.Value;
        set => _roles.Value = value;
    }
}
