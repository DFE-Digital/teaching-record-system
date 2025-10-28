using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.SupportTasks;

public static class Extensions
{
    public static IServiceCollection AddSupportTaskService(this IServiceCollection services)
    {
        services.AddTransient<SupportTaskService>();

        return services;
    }
}
