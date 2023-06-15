namespace TeachingRecordSystem.Api.Events.Processing.EventPublishing;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventPublishing(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        if (!environment.IsUnitTests())
        {
            services.AddSingleton<IHostedService, PublishEventsBackgroundService>();
        }

        services.AddSingleton<IEventObserver, NoopEventObserver>();

        return services;
    }
}
