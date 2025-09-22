using StackExchange.Redis;

namespace TeachingRecordSystem.Api.Infrastructure.Redis;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddRedis(this IHostApplicationBuilder builder)
    {
        builder.Services.AddRedis(builder.Configuration);

        return builder;
    }

    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetRequiredConnectionString("Redis");

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
        services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

        services.AddHealthChecks().AddRedis(connectionString);

        return services;
    }
}
