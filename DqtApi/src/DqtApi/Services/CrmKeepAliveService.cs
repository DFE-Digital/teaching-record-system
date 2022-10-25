using System;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DqtApi.Services
{
    public class CrmKeepAliveService : BackgroundService
    {
        private const int CallIntervalMinutes = 2;
        private const int RetryCount = 5;

        private readonly IOrganizationServiceAsync _organizationService;
        private readonly ILogger<CrmKeepAliveService> _logger;

        public CrmKeepAliveService(IOrganizationServiceAsync organizationService, ILogger<CrmKeepAliveService> logger)
        {
            _organizationService = organizationService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consecutiveFailures = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _organizationService.ExecuteAsync(new WhoAmIRequest());
                    consecutiveFailures = 0;
                }
                catch (TimeoutException ex)
                {
                    _logger.LogWarning(ex, $"Timed out executing {nameof(WhoAmIRequest)} in CRM.");

                    if (++consecutiveFailures <= RetryCount)
                    {
                        // Retry again without waiting
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error executing {nameof(WhoAmIRequest)} in CRM.");
                }

                await Task.Delay(TimeSpan.FromMinutes(CallIntervalMinutes), stoppingToken);
            }
        }
    }
}
