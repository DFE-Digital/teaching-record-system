namespace TeachingRecordSystem.SupportUi.Services.ChangeHistory;

public static class Extensions
{
    public static IServiceCollection AddChangeHistoryService(this IServiceCollection services)
    {
        services.AddTransient<ChangeHistoryService>();

        return services;
    }
}
