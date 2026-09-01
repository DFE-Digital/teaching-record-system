using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWebhookOptions(IConfiguration configuration)
        {
            services.AddOptions<WebhookOptions>()
                .Bind(configuration.GetSection("Webhooks"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }

        public IServiceCollection AddWebhookDeliveryService(IConfiguration configuration)
        {
            AddWebhookOptions(services, configuration);

            WebhookSender.Register(services);

            services.AddSingleton<IHostedService, WebhookDeliveryService>();

            return services;
        }
    }
}
