using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public static class Extensions
{
    public static IServiceCollection AddWebhookDeliveryService(this IServiceCollection services, IConfiguration configuration)
    {
        AddWebhookOptions(services, configuration);

        WebhookSender.Register(services);

        services.AddSingleton<IHostedService, WebhookDeliveryService>();

        return services;
    }

    public static IHostApplicationBuilder AddWebhookDeliveryService(this IHostApplicationBuilder builder)
    {
        AddWebhookDeliveryService(builder.Services, builder.Configuration);

        return builder;
    }

    public static IServiceCollection AddWebhookMessageEventHandlerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<WebhookMessageFactory>();
        services.AddSingleton<EventMapperRegistry>();
        services.TryAddSingleton<PersonInfoCache>();

        return services;
    }

    public static IHostApplicationBuilder AddWebhookOptions(this IHostApplicationBuilder builder)
    {
        AddWebhookOptions(builder.Services, builder.Configuration);

        return builder;
    }

    public static IServiceCollection AddWebhookOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WebhookOptions>()
            .Bind(configuration.GetSection("Webhooks"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
