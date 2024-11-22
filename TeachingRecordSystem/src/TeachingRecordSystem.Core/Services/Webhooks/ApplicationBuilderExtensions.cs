using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddWebhookOptions(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<WebhookOptions>()
            .Bind(builder.Configuration.GetSection("Webhooks"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return builder;
    }

    public static IHostApplicationBuilder AddWebhookDeliveryService(this IHostApplicationBuilder builder)
    {
        AddWebhookOptions(builder);

        builder.Services.AddSingleton<IWebhookSender, WebhookSender>();
        WebhookSender.AddHttpClient(builder.Services);

        builder.Services.AddSingleton<IHostedService, WebhookDeliveryService>();

        return builder;
    }
}
