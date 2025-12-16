using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

namespace TeachingRecordSystem.Core.Services.SupportTasks;

public static class Extensions
{
    public static IServiceCollection AddSupportTaskService(this IServiceCollection services)
    {
        services.AddTransient<SupportTaskService>();

        return services;
    }

    public static IServiceCollection AddSupportTaskServices(this IServiceCollection services)
    {
        services.AddSupportTaskService();
        services.AddOneLoginUserIdVerificationSupportTaskService();

        return services;
    }
}
