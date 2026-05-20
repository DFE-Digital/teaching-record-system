namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public static class Extensions
{
    public static IServiceCollection AddSupportTaskSearchService(this IServiceCollection services)
    {
        services.AddTransient<SupportTaskSearchService>();

        return services;
    }
}
