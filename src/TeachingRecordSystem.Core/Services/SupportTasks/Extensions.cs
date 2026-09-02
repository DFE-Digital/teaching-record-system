using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;
using TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

namespace TeachingRecordSystem.Core.Services.SupportTasks;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportTaskService()
        {
            services.AddTransient<SupportTaskService>();

            return services;
        }

        public IServiceCollection AddSupportTaskServices()
        {
            services.AddSupportTaskService();
            services.AddTransient<OneLoginUserMatchingSupportTaskService>();
            services.AddTransient<ChangeRequestSupportTaskService>();
            services.AddTransient<TeacherPensionsSupportTaskService>();

            return services;
        }
    }
}
