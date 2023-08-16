namespace TeachingRecordSystem.SupportUi.Infrastructure.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IHealthChecksBuilder healthChecksBuilder)
    {
        if (environment.IsProduction())
        {
            var connectionString = configuration.GetRequiredConnectionString("Redis");

            services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

            healthChecksBuilder.AddRedis(connectionString);
        }

        return services;
    }
}
