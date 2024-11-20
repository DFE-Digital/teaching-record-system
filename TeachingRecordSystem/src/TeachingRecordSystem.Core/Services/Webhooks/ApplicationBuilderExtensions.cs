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
}
