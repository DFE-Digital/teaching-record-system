using System.Security.Claims;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ClaimsPrincipalCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public static bool TryGetCurrentApplicationUserFromHttpContext(HttpContext httpContext, out Guid userId)
    {
        var principal = httpContext.User;

        // If there's a TRN claim then it's either an access token from ID or from Teacher Auth (i.e. AuthorizeAccess).
        if (principal.HasClaim(c => c.Type == "trn"))
        {
            if (principal.HasClaim(c => c.Type == "scope" && c.Value.Contains("dqt:read")))
            {
                // ID access token
                var idApplicationUserId = httpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue<Guid>("GetAnIdentityApplicationUserId");
                userId = idApplicationUserId;
                return true;
            }

            if (principal.FindFirstValue("trs_user_id") is string trsUserId)
            {
                // Teacher Auth access token
                userId = Guid.Parse(trsUserId);
                return true;
            }
        }

        var userIdStr = principal.FindFirstValue("sub");

        if (userIdStr is null || !Guid.TryParse(userIdStr, out userId))
        {
            userId = default;
            return false;
        }

        return true;
    }

    public Guid GetCurrentApplicationUserId()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new Exception("No HttpContext.");

        if (!TryGetCurrentApplicationUserFromHttpContext(httpContext, out var userId))
        {
            throw new Exception("No current user.");
        }

        return userId;
    }
}
