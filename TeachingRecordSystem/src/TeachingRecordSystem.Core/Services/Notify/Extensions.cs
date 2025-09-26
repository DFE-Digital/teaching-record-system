using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Notify;

public static class Extensions
{
    public static IServiceCollection AddNotifyNotificationSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NotifyOptions>()
            .Bind(configuration.GetSection("Notify"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<INotificationSender, NotificationSender>();

        return services;
    }
}
