using System.Security.Claims;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public Guid GetUserId() =>
            Guid.Parse(principal.FindFirstValue(CustomClaims.UserId) ?? throw new InvalidOperationException($"{CustomClaims.UserId} claim was not found."));

        public bool IsActiveTrsUser() =>
            principal.Claims.Any(c => c.Type == CustomClaims.UserId);
    }
}
