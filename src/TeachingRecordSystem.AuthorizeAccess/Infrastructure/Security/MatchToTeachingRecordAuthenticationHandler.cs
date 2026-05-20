using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;

public class MatchToTeachingRecordAuthenticationHandler(IJourneyInstanceProvider journeyInstanceProvider) : IAuthenticationHandler
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

            if (coordinator is null)
            {
                return AuthenticateResult.NoResult();
            }

            var ticket = coordinator.State.AuthenticationTicket;

            if (ticket is null)
            {
                return AuthenticateResult.NoResult();
            }

            return AuthenticateResult.Success(ticket);
        }
    }

    public async Task ChallengeAsync(AuthenticationProperties? properties)
    {
        EnsureInitialized();

        var coordinator = journeyInstanceProvider.GetJourneyInstance(_context) as SignInJourneyCoordinator ??
            throw new InvalidOperationException("No journey.");

        var result = coordinator.SignInWithOneLogin();
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
