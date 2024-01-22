using System.Security.Claims;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ClaimsPrincipalCurrentClientProvider(IHttpContextAccessor httpContextAccessor) : ICurrentClientProvider
{
    public static string? GetCurrentClientIdFromHttpContext(HttpContext httpContext)
    {
        var principal = httpContext.User;
        return principal.FindFirstValue("sub");
    }

    public string GetCurrentClientId()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new Exception("No HttpContext.");
        return GetCurrentClientIdFromHttpContext(httpContext) ?? throw new Exception("Current user has no Name claim.");
    }
}
