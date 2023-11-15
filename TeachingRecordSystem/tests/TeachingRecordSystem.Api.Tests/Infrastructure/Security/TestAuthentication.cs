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

        if (currentApiClientId is not null)
        {
            var principal = ApiKeyAuthenticationHandler.CreatePrincipal(currentApiClientId);

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

    [DisallowNull]
    public string? CurrentApiClientId
    {
        get => _currentApiClientId.Value;
        set => _currentApiClientId.Value = value;
    }
}
