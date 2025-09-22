using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public static class Extensions
{
    public static IServiceCollection AddWebhookOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WebhookOptions>()
            .Bind(configuration.GetSection("Webhooks"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddWebhookDeliveryService(this IServiceCollection services, IConfiguration configuration)
    {
        AddWebhookOptions(services, configuration);

        WebhookSender.Register(services);

        services.AddSingleton<IHostedService, WebhookDeliveryService>();

        return services;
    }
}
