using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;

/// <summary>
/// An <see cref="IAuthenticationSignInHandler"/> that persists an <see cref="AuthenticationTicket"/> to
/// the current FormFlow instance's state.
/// </summary>
public class FormFlowJourneySignInHandler(SignInJourneyHelper helper) : IAuthenticationSignInHandler
{
    private AuthenticationScheme? _scheme;
    private HttpContext? _context;

    public async Task<AuthenticateResult> AuthenticateAsync()
    {
        EnsureInitialized();

        var journeyInstance = await helper.UserInstanceStateProvider.GetSignInJourneyInstanceAsync(_context);

        if (journeyInstance is null || journeyInstance.State.OneLoginAuthenticationTicket is null)
        {
            return AuthenticateResult.NoResult();
        }

        return AuthenticateResult.Success(journeyInstance.State.OneLoginAuthenticationTicket);
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

        var journeyInstanceId = JourneyInstanceId.Deserialize(serializedInstanceId);

        var journeyInstance = await helper.UserInstanceStateProvider.GetSignInJourneyInstanceAsync(_context, journeyInstanceId) ??
            throw new InvalidOperationException("No FormFlow journey.");

        var ticket = new AuthenticationTicket(user, properties, _scheme.Name);

        var result = await helper.OnOneLoginCallback(journeyInstance, ticket);

        // Override the redirect done by RemoteAuthenticationHandler
        _context.Response.OnStarting(() => result.ExecuteAsync(_context));
    }

    public async Task SignOutAsync(AuthenticationProperties? properties)
    {
        EnsureInitialized();

        var journeyInstance = await helper.UserInstanceStateProvider.GetSignInJourneyInstanceAsync(_context) ??
            throw new InvalidOperationException("No FormFlow journey.");

        await journeyInstance.UpdateStateAsync(state => state.Reset());
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
