using System.Diagnostics.CodeAnalysis;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;

public class MatchToTeachingRecordAuthenticationHandler(
    SignInJourneyHelper signInJourneyHelper,
    AuthorizeAccessLinkGenerator linkGenerator) : IAuthenticationHandler
{
    private AuthenticationScheme? _scheme;
    private HttpContext? _context;

    public async Task<AuthenticateResult> AuthenticateAsync()
    {
        EnsureInitialized();

        var journeyInstance = await signInJourneyHelper.GetInstanceAsync(_context);

        if (journeyInstance is null)
        {
            return AuthenticateResult.NoResult();
        }

        var ticket = journeyInstance.State.AuthenticationTicket;

        if (ticket is null)
        {
            return AuthenticateResult.NoResult();
        }

        return AuthenticateResult.Success(ticket);
    }

    public async Task ChallengeAsync(AuthenticationProperties? properties)
    {
        EnsureInitialized();

        properties ??= new();
        properties.RedirectUri ??= "/";

        var journeyInstance = await signInJourneyHelper.GetOrCreateInstanceAsync(
            _context,
            createState: () => new SignInJourneyState(properties.RedirectUri!, OneLoginDefaults.AuthenticationScheme),
            updateState: state => state.Reset());

        properties.RedirectUri = linkGenerator.Start(journeyInstance.InstanceId);
        properties.Items.Add(FormFlowJourneySignInHandler.PropertyKeys.JourneyInstanceId, journeyInstance.InstanceId.Serialize());
        await _context!.ChallengeAsync(OneLoginDefaults.AuthenticationScheme, properties);
    }

    public Task ForbidAsync(AuthenticationProperties? properties)
    {
        throw new NotSupportedException();
    }

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        _scheme = scheme;
        _context = context;
        return Task.CompletedTask;
    }

    [MemberNotNull(nameof(_context), nameof(_scheme))]
    private void EnsureInitialized()
    {
        if (_context is null || _scheme is null)
        {
            throw new InvalidOperationException("Not initialized.");
        }
    }
}
