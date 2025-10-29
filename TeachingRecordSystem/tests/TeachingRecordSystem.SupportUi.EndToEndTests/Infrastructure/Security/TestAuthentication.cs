using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.Infrastructure.Security;

public class TestAuthenticationHandler(
    CurrentUserProvider currentUserProvider,
    IOptionsMonitor<TestAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<TestAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var currentUser = currentUserProvider.CurrentUser;

        if (currentUser is not null)
        {
            var claims = currentUser.CreateClaims()
                .Append(new Claim(ClaimTypes.NameIdentifier, currentUser.UserId.ToString()));

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
