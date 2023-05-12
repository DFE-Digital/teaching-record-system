using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;

namespace QualifiedTeachersApi.Infrastructure.Security;

public class ApiClientResolveContributor : IClientResolveContributor
{
    public Task<string> ResolveClientAsync(HttpContext httpContext)
    {
        var clientId = ClaimsPrincipalCurrentClientProvider.GetCurrentClientIdFromHttpContext(httpContext) ?? "anon";
        return Task.FromResult(clientId);
    }
}
