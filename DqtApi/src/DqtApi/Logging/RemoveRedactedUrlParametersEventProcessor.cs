using Microsoft.AspNetCore.Http;
using Sentry;
using Sentry.Extensibility;

namespace DqtApi.Logging
{
    public class RemoveRedactedUrlParametersEventProcessor : ISentryEventProcessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RemoveRedactedUrlParametersEventProcessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public SentryEvent Process(SentryEvent @event)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            @event.Request.QueryString = httpContext.Request.GetScrubbedQueryString();
            return @event;
        }
    }
}
