using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.Infrastructure.Security;

public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
{
    private readonly CurrentUserProvider _currentUserProvider;

    public TestAuthenticationHandler(
        CurrentUserProvider currentUserProvider,
        IOptionsMonitor<TestAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _currentUserProvider = currentUserProvider;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var currentUser = _currentUserProvider.CurrentUser;

        if (currentUser is not null)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, currentUser.UserId.ToString()),
                new Claim(CustomClaims.UserId, currentUser.UserId.ToString()),
                new Claim("name", currentUser.Name)
            }
            .Concat(currentUser.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, authenticationType: "Test", nameType: ClaimTypes.NameIdentifier, roleType: ClaimTypes.Role));

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

public class CurrentUserProvider
{
    [DisallowNull]
    public User? CurrentUser { get; set; }
}
