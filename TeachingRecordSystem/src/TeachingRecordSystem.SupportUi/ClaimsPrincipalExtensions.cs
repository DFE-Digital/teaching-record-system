using System.Security.Claims;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetDqtUserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue(CustomClaims.DqtUserId) ?? throw new InvalidOperationException($"{CustomClaims.DqtUserId} claim was not found."));

    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue(CustomClaims.UserId) ?? throw new InvalidOperationException($"{CustomClaims.UserId} claim was not found."));

    public static bool IsActiveTrsUser(this ClaimsPrincipal principal) =>
        principal.Claims.Any(c => c.Type == CustomClaims.UserId);
}
