using System.Security.Claims;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ClaimsPrincipalCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public static bool TryGetCurrentApplicationUserFromHttpContext(HttpContext httpContext, out (Guid UserId, string Name) user)
    {
        var principal = httpContext.User;

        // Look for ID access tokens and map those to the ID application user defined in configuration
        if (principal.HasClaim(c => c.Type == "trn") && principal.HasClaim(c => c.Type == "scope" && c.Value.Contains("dqt:read")))
        {
            var idApplicationUserId = httpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue<Guid>("GetAnIdentityApplicationUserId");
            user = (idApplicationUserId, "Get an identity");
            return true;
        }

        var userIdStr = principal.FindFirstValue("sub");
        var name = principal.FindFirstValue(ClaimTypes.Name);

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
