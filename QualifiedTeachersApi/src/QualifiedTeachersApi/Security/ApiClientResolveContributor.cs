using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;

namespace QualifiedTeachersApi.Security;

public class ApiClientResolveContributor : IClientResolveContributor
{
    public Task<string> ResolveClientAsync(HttpContext httpContext)
    {
        var clientId = ClaimsPrincipalCurrentClientProvider.GetCurrentClientIdFromHttpContext(httpContext);
        return Task.FromResult(clientId);
    }
}
