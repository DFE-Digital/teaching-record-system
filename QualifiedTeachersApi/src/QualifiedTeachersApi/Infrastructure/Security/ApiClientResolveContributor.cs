using AspNetCoreRateLimit;

namespace QualifiedTeachersApi.Infrastructure.Security;

public class ApiClientResolveContributor : IClientResolveContributor
{
    public Task<string> ResolveClientAsync(HttpContext httpContext)
    {
        var clientId = ClaimsPrincipalCurrentClientProvider.GetCurrentClientIdFromHttpContext(httpContext) ?? "anon";
        return Task.FromResult(clientId);
    }
}
