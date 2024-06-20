using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.UiCommon.FormFlow;
using static TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security.FormFlowJourneySignInHandler;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;

public sealed class OneLoginAuthenticationSchemeProvider(
        IAuthenticationSchemeProvider innerProvider,
        IConfiguration configuration,
        IDbContextFactory<TrsDbContext> dbContextFactory,
        IOptionsMonitorCache<OneLoginOptions> oneLoginOptionsMonitorCache,
        ILogger<OneLoginAuthenticationSchemeProvider> logger) :
    IAuthenticationSchemeProvider, IConfigureNamedOptions<OneLoginOptions>, IDisposable, IHostedService
{
    private static readonly TimeSpan _pollDbInterval = TimeSpan.FromMinutes(2);

    // Whether _schemeCache has been populated.
    private bool _loaded = false;

    // A map of authentication scheme names -> AuthenticationScheme + ApplicationUser.
    private readonly ConcurrentDictionary<string, AuthenticationSchemeAndApplicationUser> _schemeCache = [];

    private Task? _reloadSchemesTask;
    private CancellationTokenSource? _stoppingCts;

    private readonly ECDsaSecurityKey _coreIdentityIssuerSigningKey = GetCoreIdentityIssuerSigningKey(configuration);

    public void AddScheme(AuthenticationScheme scheme) =>
        innerProvider.AddScheme(scheme);

    public async Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
    {
        await EnsureLoaded();

        return (await innerProvider.GetAllSchemesAsync()).Concat(_schemeCache.Values.Select(v => v.Scheme));
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
        await EnsureLoaded();

        return (await innerProvider.GetRequestHandlerSchemesAsync()).Concat(_schemeCache.Values.Select(v => v.Scheme));
    }

    public async Task<AuthenticationScheme?> GetSchemeAsync(string name)
    {
        await EnsureLoaded();

        return (await innerProvider.GetSchemeAsync(name)) ??
            (_schemeCache.TryGetValue(name, out var userAndScheme) ? userAndScheme.Scheme : default);
    }

    public void RemoveScheme(string name) =>
        innerProvider.RemoveScheme(name);

    public void Dispose()
    {
        _coreIdentityIssuerSigningKey.ECDsa.Dispose();
        _stoppingCts?.Dispose();
    }

    private static ECDsaSecurityKey GetCoreIdentityIssuerSigningKey(IConfiguration configuration)
    {
        var coreIdentityIssuer = ECDsa.Create();
        var coreIdentityIssuerPem = configuration.GetRequiredValue("OneLogin:CoreIdentityIssuerPem");
        coreIdentityIssuer.ImportSubjectPublicKeyInfo(Convert.FromBase64String(coreIdentityIssuerPem), out _);
        return new ECDsaSecurityKey(coreIdentityIssuer);
    }

    private async Task EnsureLoaded()
    {
        if (!_loaded)
        {
            await ReloadSchemes();
            _loaded = true;
        }
    }

    private async Task ReloadSchemes()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var applicationUsers = await dbContext.ApplicationUsers.AsNoTracking().Where(u => u.IsOidcClient).ToListAsync();

        var seenSchemes = new List<string>();

        foreach (var user in applicationUsers)
        {
            user.EnsureConfiguredForOneLogin();
            var schemeName = user.OneLoginAuthenticationSchemeName;

            if (_schemeCache.TryGetValue(schemeName, out var schemeAndUser))
            {
                schemeAndUser.User = user;

                // Force OneLoginOptions to be recomputed for this scheme
                oneLoginOptionsMonitorCache.TryRemove(schemeName);
            }
            else
            {
                _schemeCache.TryAdd(schemeName, new() { Scheme = CreateAuthenticationScheme(user), User = user });
            }

            seenSchemes.Add(schemeName);
        }

        foreach (var kvp in _schemeCache)
        {
            if (!seenSchemes.Contains(kvp.Key))
            {
                _schemeCache.TryRemove(kvp);
            }
        }

        static AuthenticationScheme CreateAuthenticationScheme(ApplicationUser user) =>
            new(name: user.OneLoginAuthenticationSchemeName!, displayName: null, handlerType: typeof(OneLoginHandler));
    }

    private ApplicationUser GetApplicationUserForScheme(string schemeName)
    {
        return _schemeCache.TryGetValue(schemeName, out var userAndScheme) ?
            userAndScheme.User :
            throw new ArgumentException($"Scheme '{schemeName}' was not found.", nameof(schemeName));
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

            if (context.Properties!.TryGetVectorOfTrust(out var vtr) && vtr == SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr &&
                TryGetJourneyInstanceId(context.Properties, out var journeyInstanceId))
            {
                context.HandleResponse();

                var signInJourneyHelper = context.HttpContext.RequestServices.GetRequiredService<SignInJourneyHelper>();
                var journeyInstance = (await signInJourneyHelper.UserInstanceStateProvider.GetSignInJourneyInstanceAsync(context.HttpContext, journeyInstanceId))!;

                var result = await signInJourneyHelper.OnVerificationFailed(journeyInstance);
                await result.ExecuteAsync(context.HttpContext);
            }
        };

        options.CoreIdentityClaimIssuerSigningKey = _coreIdentityIssuerSigningKey;
        options.CoreIdentityClaimIssuer = "https://identity.integration.account.gov.uk/";

        options.VectorOfTrust = @"[""Cl.Cm.P2""]";

        options.Claims.Add(OneLoginClaimTypes.CoreIdentity);

        options.MetadataAddress = "https://oidc.integration.account.gov.uk/.well-known/openid-configuration";
        options.ClientAssertionJwtAudience = "https://oidc.integration.account.gov.uk/token";

        using (var rsa = RSA.Create())
        {
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
        _reloadSchemesTask = UpdateSchemesFromDb(cancellationToken);
        return Task.CompletedTask;

        async Task UpdateSchemesFromDb(CancellationToken cancellationToken)
        {
            var timer = new PeriodicTimer(_pollDbInterval);

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    await ReloadSchemes();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed refreshing One Login authentication schemes.");
                }
            }
        }
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _stoppingCts!.Cancel();
        }
        finally
        {
            await _reloadSchemesTask!.WaitAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private record AuthenticationSchemeAndApplicationUser
    {
        public required ApplicationUser User { get; set; }
        public required AuthenticationScheme Scheme { get; init; }
    };
}
