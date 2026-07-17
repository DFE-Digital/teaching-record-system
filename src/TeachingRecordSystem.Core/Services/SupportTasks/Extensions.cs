using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;
using TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

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
        services.AddTransient<OneLoginUserMatchingSupportTaskService>();
        services.AddTransient<ChangeRequestSupportTaskService>();
        services.AddTransient<TeacherPensionsSupportTaskService>();

        return services;
    }
}
