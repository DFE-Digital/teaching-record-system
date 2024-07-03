using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using TeachingRecordSystem.ServiceDefaults.Infrastructure.Logging;

namespace TeachingRecordSystem.ServiceDefaults.Infrastructure.ApplicationInsights;

public class RedactedUrlTelemetryProcessor(ITelemetryProcessor next, UrlRedactor urlRedactor) : ITelemetryProcessor
{
    public void Process(ITelemetry item)
    {
        if (item is RequestTelemetry requestTelemetry)
        {
            requestTelemetry.Url = new Uri(urlRedactor.GetScrubbedRequestUrl());
        }

        next.Process(item);
    }
}
