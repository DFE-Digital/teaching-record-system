using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;

public class MatchToTeachingRecordAuthenticationHandler(SignInJourneyHelper helper) : IAuthenticationHandler
{
    private AuthenticationScheme? _scheme;
    private HttpContext? _context;

    public async Task<AuthenticateResult> AuthenticateAsync()
    {
        EnsureInitialized();

        var journeyInstance = await helper.UserInstanceStateProvider.GetSignInJourneyInstanceAsync(_context);

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

        if (properties is null ||
            !properties.Items.TryGetValue("OneLoginAuthenticationScheme", out var oneLoginAuthenticationScheme) ||
            string.IsNullOrEmpty(oneLoginAuthenticationScheme))
        {
            throw new InvalidOperationException($"'OneLoginAuthenticationScheme' must be passed in {nameof(properties)}.{nameof(properties.Items)}.");
        }

        var journeyInstance = await helper.UserInstanceStateProvider.GetOrCreateSignInJourneyInstanceAsync(
            _context,
            createState: () => new SignInJourneyState(properties.RedirectUri ?? "/", oneLoginAuthenticationScheme, properties),
            updateState: state => state.Reset());

        var result = helper.SignInWithOneLogin(journeyInstance);
        await result.ExecuteAsync(_context);
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
