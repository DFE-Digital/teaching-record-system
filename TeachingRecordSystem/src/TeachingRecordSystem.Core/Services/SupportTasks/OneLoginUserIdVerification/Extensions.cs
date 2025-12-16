using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

public static class Extensions
{
    public static IServiceCollection AddOneLoginUserIdVerificationSupportTaskService(this IServiceCollection services)
    {
        services.AddTransient<OneLoginUserIdVerificationSupportTaskService>();

        return services;
    }
}
