using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;

/// <summary>
/// An <see cref="IAuthenticationSignInHandler"/> that persists an <see cref="AuthenticationTicket"/> to
/// the current journey's state.
/// </summary>
public class JourneySignInHandler(IJourneyInstanceProvider journeyInstanceProvider) : IAuthenticationSignInHandler
{
    private AuthenticationScheme? _scheme;
    private HttpContext? _context;

    public Task<AuthenticateResult> AuthenticateAsync()
    {
        EnsureInitialized();

        return Task.FromResult(Impl());

        AuthenticateResult Impl()
        {
            var coordinator = journeyInstanceProvider.GetJourneyInstance(_context) as SignInJourneyCoordinator;

            if (coordinator?.State.OneLoginAuthenticationTicket is null)
            {
                return AuthenticateResult.NoResult();
            }

            return AuthenticateResult.Success(coordinator.State.OneLoginAuthenticationTicket);
        }
    }

    public async Task ChallengeAsync(AuthenticationProperties? properties)
    {
        // We'll get here if an instance ID wasn't provided in the query string
        // (and MissingInstanceFilter won't have executed yet since authorization runs before resource filters).
        EnsureInitialized();
        var result = Results.BadRequest();
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

    public async Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        EnsureInitialized();

        if (properties is null ||
            !properties.Items.TryGetValue(PropertyKeys.JourneyInstanceId, out var serializedInstanceId) ||
            serializedInstanceId is null)
        {
            throw new InvalidOperationException($"{PropertyKeys.JourneyInstanceId} must be specified in {nameof(properties)}.");
        }

        var coordinator = (SignInJourneyCoordinator?)journeyInstanceProvider.GetJourneyInstance(_context) ??
            throw new InvalidOperationException("No journey.");

        var ticket = new AuthenticationTicket(user, properties, _scheme.Name);

        var result = await coordinator.OnOneLoginCallbackAsync(ticket);

        await result.ExecuteAsync(_context);
    }

    public Task SignOutAsync(AuthenticationProperties? properties)
    {
        EnsureInitialized();

        var coordinator = journeyInstanceProvider.GetJourneyInstance(_context) as SignInJourneyCoordinator ??
            throw new InvalidOperationException("No journey.");

        coordinator.OnSignOut();

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

    public static class PropertyKeys
    {
        public const string JourneyInstanceId = nameof(JourneyInstanceId);
    }
}
