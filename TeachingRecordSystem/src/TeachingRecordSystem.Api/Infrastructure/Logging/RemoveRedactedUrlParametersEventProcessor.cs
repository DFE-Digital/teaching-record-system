using Sentry;
using Sentry.Extensibility;

namespace TeachingRecordSystem.Api.Infrastructure.Logging;

public class RemoveRedactedUrlParametersEventProcessor : ISentryEventProcessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RemoveRedactedUrlParametersEventProcessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public SentryEvent Process(SentryEvent @event)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new System.Exception("No HttpContext.");
        @event.Request.QueryString = httpContext.Request.GetScrubbedQueryString();
        return @event;
    }
}
