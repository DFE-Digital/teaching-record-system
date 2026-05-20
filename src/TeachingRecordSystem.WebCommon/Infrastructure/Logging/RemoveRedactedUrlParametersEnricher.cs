using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace TeachingRecordSystem.WebCommon.Infrastructure.Logging;

public class RemoveRedactedUrlParametersEnricher(IHttpContextAccessor httpContextAccessor, UrlRedactor urlRedactor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return;
        }

        foreach (var (key, value) in logEvent.Properties)
        {
            if (value is ScalarValue scalarValue && scalarValue.Value is string stringValue)
            {
                var newValue = stringValue;

                var requestPath = httpContext.Request.Path;
                if (stringValue.Contains(requestPath))
                {
                    var scrubbedRequestPath = urlRedactor.GetScrubbedRequestPath();
                    newValue = stringValue.Replace(requestPath, scrubbedRequestPath);
                }

                var queryString = httpContext.Request.QueryString.ToString();
                if (queryString is not "" && stringValue.Contains(queryString.TrimStart('?')))
                {
                    var scrubbedQueryString = urlRedactor.GetScrubbedQueryString();
                    newValue = newValue.Replace(queryString.TrimStart('?'), scrubbedQueryString.TrimStart('?'));
                }

                if (newValue != stringValue)
                {
                    logEvent.AddOrUpdateProperty(new LogEventProperty(key, new ScalarValue(newValue)));
                }
            }
        }
    }
}
