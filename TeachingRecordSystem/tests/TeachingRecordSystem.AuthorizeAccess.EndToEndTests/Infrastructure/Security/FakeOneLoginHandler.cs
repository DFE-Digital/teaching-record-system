using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests.Infrastructure.Security;

public class FakeOneLoginHandler(OneLoginCurrentUserProvider currentUserProvider) : IAuthenticationHandler, IAuthenticationSignOutHandler
{
    private HttpContext? _context;

    public Task<AuthenticateResult> AuthenticateAsync()
    {
        _ = _context ?? throw new InvalidOperationException("Not initialized.");

        return _context.AuthenticateAsync(AuthenticationSchemes.FormFlowJourney);
    }

    public async Task ChallengeAsync(AuthenticationProperties? properties)
    {
        _ = _context ?? throw new InvalidOperationException("Not initialized.");
        var user = currentUserProvider.CurrentUser ?? throw new InvalidOperationException("No current user set.");

        var claims = new List<Claim>()
        {
            new("sub", user.sub),
            new("email", user.email),
            new("vot", user.vot),
            new("sid", user.sid)
        };

        if (user.CoreIdentityVc is not null)
        {
            claims.Add(new Claim("vc", user.CoreIdentityVc, "JSON"));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Fake One Login", nameType: "sub", roleType: null));

        var authenticatedProperties = properties?.Clone() ?? new();
        authenticatedProperties.StoreTokens([new AuthenticationToken() { Name = OpenIdConnectParameterNames.IdToken, Value = "dummy" }]);

        await _context.SignInAsync(AuthenticationSchemes.FormFlowJourney, principal, authenticatedProperties);
    }

    public Task ForbidAsync(AuthenticationProperties? properties) =>
        throw new NotSupportedException();

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        _context = context;
        return Task.CompletedTask;
    }

    public Task SignOutAsync(AuthenticationProperties? properties)
    {
        return Task.CompletedTask;
    }
}

public class OneLoginCurrentUserProvider
{
    [DisallowNull]
    public OneLoginUserInfo? CurrentUser { get; set; }
}
