using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.Tests.Infrastructure.Security;

public class TestApiKeyAuthenticationHandler(
    CurrentApiClientProvider currentApiClientProvider,
    IOptionsMonitor<TestApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<TestApiKeyAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Use-CurrentClientIdProvider", out var useCurrentClientIdProvider) ||
            useCurrentClientIdProvider != "true")
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var currentApiClientId = currentApiClientProvider.CurrentApiUserId;
        var currentRoles = currentApiClientProvider.Roles!;

        if (currentApiClientId is Guid id)
        {
            var principal = ApiKeyAuthenticationHandler.CreatePrincipal(id, name: id.ToString(), currentRoles);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        else
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}

public class TestApiKeyAuthenticationOptions : AuthenticationSchemeOptions { }

public class CurrentApiClientProvider
{
    private readonly AsyncLocal<Guid?> _currentApiUserId = new();
    private readonly AsyncLocal<string[]> _roles = new();

    [DisallowNull]
    public Guid? CurrentApiUserId
    {
        get => _currentApiUserId.Value;
        set => _currentApiUserId.Value = value;
    }

    [DisallowNull]
    public string[]? Roles
    {
        get => _roles.Value;
        set => _roles.Value = value;
    }
}
