using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Events.Processing.EventPublishing;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddEventPublishing(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests())
        {
            builder.Services.AddSingleton<IHostedService, PublishEventsBackgroundService>();
        }

        return builder;
    }
}
