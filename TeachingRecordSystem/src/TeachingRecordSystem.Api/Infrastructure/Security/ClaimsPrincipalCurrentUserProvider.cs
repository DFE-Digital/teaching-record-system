using System.Security.Claims;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ClaimsPrincipalCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public static bool TryGetCurrentApplicationUserFromHttpContext(HttpContext httpContext, out (Guid UserId, string Name) user)
    {
        var userIdStr = httpContext.User.FindFirstValue("sub");
        var name = httpContext.User.FindFirstValue(ClaimTypes.Name);

        if (userIdStr is null || !Guid.TryParse(userIdStr, out var userId) || name is null)
        {
            user = default;
            return false;
        }

        user = (UserId: userId, Name: name);
        return true;
    }

    public (Guid UserId, string Name) GetCurrentApplicationUser()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new Exception("No HttpContext.");

        if (!TryGetCurrentApplicationUserFromHttpContext(httpContext, out var user))
        {
            throw new Exception("No current user.");
        }

        return user;
    }
}
