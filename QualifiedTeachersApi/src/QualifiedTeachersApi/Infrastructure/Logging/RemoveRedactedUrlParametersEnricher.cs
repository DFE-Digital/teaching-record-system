#nullable disable
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace QualifiedTeachersApi.Infrastructure.Logging;

public class RemoveRedactedUrlParametersEnricher : ILogEventEnricher
{
    private readonly HttpContext _httpContext;

    public RemoveRedactedUrlParametersEnricher(HttpContext httpContext)
    {
        _httpContext = httpContext;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        const string requestPathPropertyName = "RequestPath";

        if (logEvent.Properties.ContainsKey(requestPathPropertyName))
        {
            var scrubbedRequestPath = _httpContext.Request.GetScrubbedRequestPathAndQuery();
            logEvent.AddOrUpdateProperty(new LogEventProperty(requestPathPropertyName, new ScalarValue(scrubbedRequestPath)));
        }
    }
}
