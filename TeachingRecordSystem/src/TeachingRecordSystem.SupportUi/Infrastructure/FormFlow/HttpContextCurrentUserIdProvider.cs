namespace TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

public class HttpContextCurrentUserIdProvider : ICurrentUserIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No current HttpContext.");
        return httpContext.User.GetUserId();
    }
}
