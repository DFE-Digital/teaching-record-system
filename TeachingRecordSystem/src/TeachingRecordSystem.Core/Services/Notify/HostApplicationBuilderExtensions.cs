using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.Notify;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddEmail(this IHostApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            builder.Services.AddOptions<NotifyOptions>()
                .Bind(builder.Configuration.GetSection("Notify"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddSingleton<INotificationSender, NotificationSender>();
        }
        else
        {
            builder.Services.AddSingleton<INotificationSender, NoopNotificationSender>();
        }

        return builder;
    }
}
