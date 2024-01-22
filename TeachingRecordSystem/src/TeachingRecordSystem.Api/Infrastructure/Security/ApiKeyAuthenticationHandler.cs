using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Serilog.Context;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ApiKeyAuthenticationHandler(
    TrsDbContext dbContext,
    IMemoryCache memoryCache,
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "ApiKey";

    private const string AuthenticationHeaderScheme = "Bearer";

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? authorizationHeader = Request.Headers.Authorization;

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return AuthenticateResult.NoResult();
        }

        if (!authorizationHeader.StartsWith(AuthenticationHeaderScheme + " ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var key = authorizationHeader[(AuthenticationHeaderScheme.Length + 1)..];

        var apiKey = await memoryCache.GetOrCreateAsync(
            key: $"ApiKeys:{key}",
            async cacheEntry =>
            {
                var apiKey = await dbContext.ApiKeys
                    .Where(k => k.Key == key)
                    .Include(k => k.ApplicationUser)
                    .SingleOrDefaultAsync();

                if (apiKey is null)
                {
                    // Don't cache bad keys
                    cacheEntry.SetAbsoluteExpiration(DateTimeOffset.UtcNow);
                }
                else
                {
                    cacheEntry.SetSlidingExpiration(TimeSpan.FromMinutes(2));
                }

                return apiKey;
            });

        if (apiKey is null)
        {
            return AuthenticateResult.Fail($"No client found with specified API key.");
        }

        if (apiKey.Expires <= DateTime.UtcNow)
        {
            return AuthenticateResult.Fail($"API key is expired.");
        }

        var principal = CreatePrincipal(apiKey.ApplicationUserId.ToString(), apiKey.ApplicationUser.Name, apiKey.ApplicationUser.ApiRoles);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        LogContext.PushProperty("ClientId", apiKey.ApplicationUser.UserId);

        return AuthenticateResult.Success(ticket);
    }

    public static ClaimsPrincipal CreatePrincipal(string clientId, string name, IEnumerable<string> roles)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", clientId),
            new Claim(ClaimTypes.Name, name)
        });

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(identity);
    }
}
