using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.WebCommon.FormFlow;
using static TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security.FormFlowJourneySignInHandler;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;

public sealed class OneLoginAuthenticationSchemeProvider(
        IAuthenticationSchemeProvider innerProvider,
        IDbContextFactory<TrsDbContext> dbContextFactory,
        IOptions<AuthorizeAccessOptions> authorizeAccessOptionsAccessor,
        IOptionsMonitorCache<OneLoginOptions> oneLoginOptionsMonitorCache,
        ILogger<OneLoginAuthenticationSchemeProvider> logger) :
    IAuthenticationSchemeProvider, IConfigureNamedOptions<OneLoginOptions>, IDisposable, IHostedService
{
    private IReadOnlyCollection<AuthenticationSchemeAndApplicationUser> _schemeCache = [];

    private CancellationTokenSource? _stoppingCts;

    public void AddScheme(AuthenticationScheme scheme) =>
        innerProvider.AddScheme(scheme);

    public async Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
    {
        var innerProviderSchemes = await innerProvider.GetAllSchemesAsync();

        lock (_schemeCache)
        {
            return innerProviderSchemes.Concat(_schemeCache.Select(v => v.Scheme));
        }
    }

    public Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync() =>
        innerProvider.GetDefaultAuthenticateSchemeAsync();

    public Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync() =>
        innerProvider.GetDefaultChallengeSchemeAsync();

    public Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync() =>
        innerProvider.GetDefaultForbidSchemeAsync();

    public Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync() =>
        innerProvider.GetDefaultSignInSchemeAsync();

    public Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync() =>
        innerProvider.GetDefaultSignOutSchemeAsync();

    public async Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
    {
        var innerProviderSchemes = await innerProvider.GetRequestHandlerSchemesAsync();

        lock (_schemeCache)
        {
            return innerProviderSchemes.Concat(_schemeCache.Select(v => v.Scheme));
        }
    }

    public async Task<AuthenticationScheme?> GetSchemeAsync(string name)
    {
        if (await innerProvider.GetSchemeAsync(name) is { } scheme)
        {
            return scheme;
        }

        lock (_schemeCache)
        {
            return _schemeCache.SingleOrDefault(v => v.Scheme.Name == name)?.Scheme;
        }
    }

    public void RemoveScheme(string name) =>
        innerProvider.RemoveScheme(name);

    public void Dispose()
    {
        _stoppingCts?.Dispose();
    }

    private async Task ReloadSchemesAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var applicationUsers = await dbContext.ApplicationUsers.AsNoTracking().Where(u => u.IsOidcClient).ToArrayAsync();

        var newSchemes = new List<AuthenticationSchemeAndApplicationUser>();
        foreach (var user in applicationUsers)
        {
            user.EnsureConfiguredForOneLogin();
            newSchemes.Add(new(user, CreateAuthenticationScheme(user)));
        }

        var oldAndNewSchemes = new HashSet<string>();
        lock (_schemeCache)
        {
            foreach (var (_, scheme) in _schemeCache)
            {
                oldAndNewSchemes.Add(scheme.Name);
            }

            _schemeCache = newSchemes;
        }

        foreach (var scheme in oldAndNewSchemes)
        {
            // Force OneLoginOptions to be recomputed for all schemes
            oneLoginOptionsMonitorCache.TryRemove(scheme);
        }

        static AuthenticationScheme CreateAuthenticationScheme(ApplicationUser user) =>
            new(name: user.OneLoginAuthenticationSchemeName!, displayName: null, handlerType: typeof(OneLoginHandler));
    }

    private ApplicationUser GetApplicationUserForScheme(string schemeName)
    {
        lock (_schemeCache)
        {
            return _schemeCache.SingleOrDefault(v => v.Scheme.Name == schemeName)?.User ??
                throw new ArgumentException($"Scheme '{schemeName}' was not found.", nameof(schemeName));
        }
    }

    void IConfigureNamedOptions<OneLoginOptions>.Configure(string? name, OneLoginOptions options)
    {
        if (name is null)
        {
            return;
        }

        var user = GetApplicationUserForScheme(name);
        user.EnsureConfiguredForOneLogin();

        options.SignInScheme = AuthenticationSchemes.FormFlowJourney;
        options.UseJwtSecuredAuthorizationRequest = false;  // TEMP, the default is currently broken in OneLogin.AspNetCore package

        options.Events.OnRedirectToIdentityProviderForSignOut = context =>
        {
            // The standard sign out process will call Authenticate() on SignInScheme then try to extract the id_token from the Principal.
            // That won't work in our case most of the time since sign out journeys won't have the FormFlow instance around that has the AuthenticationTicket.
            // Instead, we'll get it passed to us in explicitly in AuthenticationProperties.Items.

            if (context.ProtocolMessage.IdTokenHint is null &&
                context.Properties.Parameters.TryGetValue(OpenIdConnectParameterNames.IdToken, out var idToken) &&
                idToken is string idTokenString)
            {
                context.ProtocolMessage.IdTokenHint = idTokenString;
            }

            return Task.CompletedTask;
        };

        options.Events.OnAccessDenied = async context =>
        {
            // This handles the scenario where we've requested ID verification but One Login couldn't do it.

            if (context.Properties!.TryGetVectorsOfTrust(out var vtr) && vtr.SequenceEqual([SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr]) &&
                TryGetJourneyInstanceId(context.Properties, out var journeyInstanceId))
            {
                context.HandleResponse();

                var signInJourneyHelper = context.HttpContext.RequestServices.GetRequiredService<SignInJourneyHelper>();
                var journeyInstance = (await signInJourneyHelper.UserInstanceStateProvider.GetSignInJourneyInstanceAsync(context.HttpContext, journeyInstanceId))!;

                var result = await signInJourneyHelper.OnVerificationFailedAsync(journeyInstance);
                await result.ExecuteAsync(context.HttpContext);
            }
        };

        options.VectorsOfTrust = ["Cl.Cm.P2"];

        options.Claims.Add(OneLoginClaimTypes.CoreIdentity);

        options.Environment = OneLoginEnvironments.Integration;

        if (user.UseSharedOneLoginSigningKeys)
        {
            var key = authorizeAccessOptionsAccessor.Value.GetOneLoginSigningCredentials();
            options.ClientAuthenticationCredentials = key;
        }
        else
        {
            using var rsa = RSA.Create();
            var privateKeyPem = user.OneLoginPrivateKeyPem;
            rsa.ImportFromPem(privateKeyPem);

            options.ClientAuthenticationCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true)),
                SecurityAlgorithms.RsaSha256);
        }

        options.ClientId = user.OneLoginClientId;
        options.CallbackPath = EnsurePrefixedWithSlash(user.OneLoginRedirectUriPath);
        options.SignedOutCallbackPath = EnsurePrefixedWithSlash(user.OneLoginPostLogoutRedirectUriPath);

        options.CorrelationCookie.Name = "onelogin-correlation.";
        options.NonceCookie.Name = "onelogin-nonce.";

        static string EnsurePrefixedWithSlash(string value) => !value.StartsWith('/') ? "/" + value : value;

        static bool TryGetJourneyInstanceId(
            AuthenticationProperties? properties,
            [NotNullWhen(true)] out JourneyInstanceId? journeyInstanceId)
        {
            if (properties?.Items.TryGetValue(PropertyKeys.JourneyInstanceId, out var serializedInstanceId) == true
                && serializedInstanceId is not null)
            {
                journeyInstanceId = JourneyInstanceId.Deserialize(serializedInstanceId);
                return true;
            }

            journeyInstanceId = default;
            return false;
        }
    }

    void IConfigureOptions<OneLoginOptions>.Configure(OneLoginOptions options)
    {
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = RefreshOnNotificationAsync(_stoppingCts.Token);
        return ReloadSchemesAsync();

        async Task RefreshOnNotificationAsync(CancellationToken cancellationToken)
        {
            while (!_stoppingCts.Token.IsCancellationRequested)
            {
                try
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();

                    conn.Notification += (sender, args) => _ = ReloadSchemesAsync();

                    await conn.OpenAsync(cancellationToken);

                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"LISTEN {ChannelNames.OneLoginClient};";
                    await cmd.ExecuteNonQueryAsync(cancellationToken);

                    while (true)
                    {
                        await conn.WaitAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed waiting for notifications from {ChannelNames.OneLoginClient}.");
                }
            }
        }
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => _stoppingCts!.CancelAsync();

    private record AuthenticationSchemeAndApplicationUser(ApplicationUser User, AuthenticationScheme Scheme);
}
