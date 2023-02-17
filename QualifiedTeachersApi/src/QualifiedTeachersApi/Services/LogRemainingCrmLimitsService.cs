using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using QualifiedTeachersApi.DataStore.Crm;

namespace QualifiedTeachersApi.Services;

public class LogRemainingCrmLimitsService : BackgroundService
{
    private static readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);

    private readonly IWebApiAdapter _webApiAdapter;
    private readonly ILogger<LogRemainingCrmLimitsService> _logger;
    private readonly Gauge _remainingRequestsMetric;
    private readonly Gauge _remainingExecutionTimeMetric;

    public LogRemainingCrmLimitsService(IWebApiAdapter webApiAdapter, ILogger<LogRemainingCrmLimitsService> logger)
    {
        _webApiAdapter = webApiAdapter;
        _logger = logger;
        _remainingRequestsMetric = Metrics.CreateGauge("crm_remaining_requests", "Remaining CRM requests");
        _remainingExecutionTimeMetric = Metrics.CreateGauge("crm_remaining_execution_time", "Remaining CRM execution time");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(_pollInterval);

        do
        {
            try
            {
                var limits = await _webApiAdapter.GetRemainingApiLimits();

                _remainingRequestsMetric.Set(limits.NumberOfRequests);
                _remainingExecutionTimeMetric.Set(limits.RemainingExecutionTime);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed retrieving CRM API limits.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
