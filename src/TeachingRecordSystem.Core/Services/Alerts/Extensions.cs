using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Alerts;

public static class Extensions
{
    public static IServiceCollection AddAlertService(this IServiceCollection services)
    {
        services.AddTransient<AlertService>();

        return services;
    }
}
