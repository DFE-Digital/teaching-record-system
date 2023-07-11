using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.Notify;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmail(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("RecurringJobs:Enabled"))
        {
            if (environment.IsProduction())
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
        }

        return services;
    }
}
