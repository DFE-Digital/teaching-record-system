using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Serilog.Context;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ApiKeyAuthenticationHandler(
    TrsDbContext dbContext,
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "ApiKey";

    private const string AuthenticationHeaderScheme = "Bearer";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
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

        var apiKey = await dbContext.ApiKeys
            .AsNoTracking()
            .Where(k => k.Key == key)
            .Include(k => k.ApplicationUser)
            .SingleOrDefaultAsync();

        if (apiKey is null)
        {
            return AuthenticateResult.Fail("No client found with specified API key.");
        }

        if (apiKey.Expires <= DateTime.UtcNow)
        {
            return AuthenticateResult.Fail("API key is expired.");
        }

        var applicationUser = apiKey.ApplicationUser!;

        var principal = CreatePrincipal(applicationUser.UserId, applicationUser.Name, applicationUser.ApiRoles ?? []);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        LogContext.PushProperty("ApplicationUserId", applicationUser.UserId);

        SentrySdk.ConfigureScope(scope => scope.User = new SentryUser
        {
            Id = applicationUser.UserId.ToString(),
            Username = applicationUser.Name
        });

        return AuthenticateResult.Success(ticket);
    }

    public static ClaimsPrincipal CreatePrincipal(Guid applicationUserId, string name, IEnumerable<string> roles)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim("sub", applicationUserId.ToString()),
                new Claim(ClaimTypes.Name, name)
            ],
            authenticationType: "Bearer",
            nameType: "sub",
            roleType: ClaimTypes.Role);

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(identity);
    }
}
