namespace QualifiedTeachersApi.Services.Notify;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmail(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsProduction() || true)
        {
            services.AddOptions<NotifyOptions>()
                .Bind(configuration.GetSection("Notify"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<INotificationSender, NotificationSender>();
        }
        else
        {
            services.AddSingleton<INotificationSender, NoopNotificationSender>();
        }

        return services;
    }
}
