using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Events.Processing.EventPublishing;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventPublishing(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        if (!environment.IsUnitTests())
        {
            services.AddSingleton<IHostedService, PublishEventsBackgroundService>();
        }

        services.AddSingleton<IEventObserver, NoopEventObserver>();

        return services;
    }
}
