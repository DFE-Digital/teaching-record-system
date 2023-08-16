using System.Security.Claims;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ClaimsPrincipalCurrentClientProvider : ICurrentClientProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsPrincipalCurrentClientProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public static string? GetCurrentClientIdFromHttpContext(HttpContext httpContext)
    {
        var principal = httpContext.User;
        return principal.FindFirstValue(ClaimTypes.Name);
    }

    public string GetCurrentClientId()
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new Exception("No HttpContext.");
        return GetCurrentClientIdFromHttpContext(httpContext) ?? throw new Exception("Current user has no Name claim.");
    }
}
