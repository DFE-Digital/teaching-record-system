#nullable enable
using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

public class IgnoreDependencyTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;

    public IgnoreDependencyTelemetryProcessor(ITelemetryProcessor next)
    {
        _next = next;
    }

    public void Process(ITelemetry item)
    {
        bool isFromDqtReportingService = false;

        Activity? activity = Activity.Current;
        while (activity is not null)
        {
            if (activity.GetTagItem("OperationName") as string == DqtReportingService.ProcessChangesOperationName)
            {
                isFromDqtReportingService = true;
                break;
            }

            activity = activity.Parent;
        }

        if (!isFromDqtReportingService || item is not DependencyTelemetry)
        {
            _next.Process(item);
        }
    }
}
