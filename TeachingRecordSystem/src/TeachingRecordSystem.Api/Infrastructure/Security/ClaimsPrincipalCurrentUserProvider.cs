using System.Security.Claims;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ClaimsPrincipalCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public static bool TryGetCurrentClientIdFromHttpContext(HttpContext httpContext, out Guid userId)
    {
        var userIdStr = httpContext.User.FindFirstValue("sub");

        if (userIdStr is null)
        {
            userId = default;
            return false;
        }

        return Guid.TryParse(userIdStr, out userId);
    }

    public Guid GetCurrentApplicationUserId()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new Exception("No HttpContext.");

        if (!TryGetCurrentClientIdFromHttpContext(httpContext, out var userId))
        {
            throw new Exception("Current user has no 'sub' claim.");
        }

        return userId;
    }
}
