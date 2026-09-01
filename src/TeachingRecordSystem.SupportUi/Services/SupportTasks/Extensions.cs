namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportTaskSearchService()
        {
            services.AddTransient<SupportTaskSearchService>();

            return services;
        }

        public IServiceCollection AddSupportTaskAssignmentOptions(IConfiguration configuration)
        {
            services.AddOptions<SupportTaskAssignmentOptions>()
                .Bind(configuration.GetSection("SupportTaskAssignment"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }
    }
}
