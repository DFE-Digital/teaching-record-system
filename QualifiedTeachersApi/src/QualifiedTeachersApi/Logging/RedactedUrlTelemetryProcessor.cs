using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace QualifiedTeachersApi.Logging
{
    public class RedactedUrlTelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RedactedUrlTelemetryProcessor(ITelemetryProcessor next, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Process(ITelemetry item)
        {
            if (item is RequestTelemetry requestTelemetry)
            {
                requestTelemetry.Url = new Uri(_httpContextAccessor.HttpContext.Request.GetScrubbedRequestUrl());
            }

            _next.Process(item);
        }
    }
}
