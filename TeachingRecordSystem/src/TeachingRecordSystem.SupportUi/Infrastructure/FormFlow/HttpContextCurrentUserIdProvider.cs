namespace TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

public class HttpContextCurrentUserIdProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserIdProvider
{
    public string GetCurrentUserId()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No current HttpContext.");
        return httpContext.User.GetUserId().ToString();
    }
}
