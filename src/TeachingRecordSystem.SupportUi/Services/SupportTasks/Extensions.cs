namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public static class Extensions
{
    public static IServiceCollection AddSupportTaskSearchService(this IServiceCollection services)
    {
        services.AddTransient<SupportTaskSearchService>();

        return services;
    }

    public static IServiceCollection AddSupportTaskAssignmentOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SupportTaskAssignmentOptions>()
            .Bind(configuration.GetSection("SupportTaskAssignment"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
