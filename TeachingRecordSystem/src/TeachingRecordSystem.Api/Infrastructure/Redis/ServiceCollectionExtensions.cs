using StackExchange.Redis;

namespace TeachingRecordSystem.Api.Infrastructure.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsProduction())
        {
            var connectionString = configuration.GetRequiredConnectionString("Redis");

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
            services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

            services.AddHealthChecks().AddRedis(connectionString);
        }

        return services;
    }
}
