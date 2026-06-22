namespace TeachingRecordSystem.SupportUi.Services.OneLogins;

public static class Extensions
{
    public static IServiceCollection AddOneLoginSearchService(this IServiceCollection services)
    {
        services.AddTransient<OneLoginSearchService>();

        return services;
    }
}
