using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests.Infrastructure.Security;

public class FakeOneLoginHandler(OneLoginCurrentUserProvider currentUserProvider, SignInJourneyHelper signInJourneyHelper) : IAuthenticationHandler, IAuthenticationSignOutHandler
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

        if (properties is null || !properties.TryGetVectorOfTrust(out var vtr))
        {
            throw new InvalidOperationException("No vtr set.");
        }

        if (vtr == SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr && user.CoreIdentityVc is null)
        {
            // Simulate an 'access_denied' error for failing ID verification

            if (!properties.Items.TryGetValue(FormFlowJourneySignInHandler.PropertyKeys.JourneyInstanceId, out var serializedInstanceId) || serializedInstanceId is null)
            {
                throw new InvalidOperationException("No JourneyInstanceId set.");
            }

            var journeyInstanceId = JourneyInstanceId.Deserialize(serializedInstanceId);

            var journeyInstance = (await signInJourneyHelper.UserInstanceStateProvider.GetSignInJourneyInstanceAsync(_context, journeyInstanceId))!;

            var result = await signInJourneyHelper.OnVerificationFailed(journeyInstance);
            await result.ExecuteAsync(_context);

            return;
        }

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
