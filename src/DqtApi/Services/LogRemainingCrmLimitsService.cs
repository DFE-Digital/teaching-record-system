using System;
using System.Threading;
using DqtApi.DataStore.Crm;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace DqtApi.Services
{
    public class LogRemainingCrmLimitsService : BackgroundService
    {
        private static readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);

        private readonly IWebApiAdapter _webApiAdapter;
        private readonly Gauge _remainingRequestsMetric;
        private readonly Gauge _remainingExecutionTimeMetric;

        public LogRemainingCrmLimitsService(IWebApiAdapter webApiAdapter)
        {
            _webApiAdapter = webApiAdapter;

            _remainingRequestsMetric = Metrics.CreateGauge("crm_remaining_requests", "Remaining CRM requests");
            _remainingExecutionTimeMetric = Metrics.CreateGauge("crm_remaining_execution_time", "Remaining CRM execution time");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(_pollInterval);

            do
            {
                var limits = await _webApiAdapter.GetRemainingApiLimits();

                _remainingRequestsMetric.Set(limits.NumberOfRequests);
                _remainingExecutionTimeMetric.Set(limits.RemainingExecutionTime);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
