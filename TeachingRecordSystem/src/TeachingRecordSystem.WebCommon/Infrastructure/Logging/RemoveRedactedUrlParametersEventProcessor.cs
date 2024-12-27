using Microsoft.AspNetCore.Http;
using Sentry.Extensibility;

namespace TeachingRecordSystem.WebCommon.Infrastructure.Logging;

public class RemoveRedactedUrlParametersEventProcessor(IHttpContextAccessor httpContextAccessor, UrlRedactor urlRedactor) : ISentryEventProcessor
{
    public SentryEvent Process(SentryEvent @event)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            @event.Request.QueryString = urlRedactor.GetScrubbedQueryString();
            @event.Request.Url = urlRedactor.GetScrubbedRequestUrl();
        }

        return @event;
    }
}
