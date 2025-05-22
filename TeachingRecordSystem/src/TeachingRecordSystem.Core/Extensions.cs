using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;
using Serilog.Formatting.Compact;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core;

public static class Extensions
{
    public static IHostApplicationBuilder AddBackgroundWorkScheduler(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddSingleton<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        var pgConnectionString = GetPostgresConnectionString(builder.Configuration);

        builder.Services.AddDatabase(pgConnectionString);

        return builder;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddNpgsqlDataSource(connectionString, builder => builder.Name = "TrsDb");

        services.AddDbContext<TrsDbContext>(
            options => TrsDbContext.ConfigureOptions(options),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<TrsDbContext>(options => TrsDbContext.ConfigureOptions(options));

        return services;
    }

    public static IHostApplicationBuilder AddHangfire(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddHangfire((sp, configuration) => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o => o.UseConnectionFactory(
                    new DbDataSourceConnectionFactory(sp.GetRequiredService<NpgsqlDataSource>())),
                    new PostgreSqlStorageOptions()
                    {
                        UseSlidingInvisibilityTimeout = true
                    }));
        }

        return builder;
    }

    public static IHostApplicationBuilder AddWebhookMessageFactory(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<WebhookMessageFactory>();
        builder.Services.AddSingleton<EventMapperRegistry>();
        builder.Services.TryAddSingleton<PersonInfoCache>();

        return builder;
    }

    public static void ConfigureSerilog(
        this LoggerConfiguration config,
        IHostEnvironment environment,
        IConfiguration configuration,
        IServiceProvider services)
    {
        config
            .ReadFrom.Configuration(configuration)
            .WriteTo.Sentry(o => o.InitializeSdk = false);

        if (environment.IsProduction())
        {
            config.WriteTo.Console(new CompactJsonFormatter());
        }
        else
        {
            config.WriteTo.Console();
        }
    }

    public static string GetPostgresConnectionString(this IConfiguration configuration) =>
        new NpgsqlConnectionStringBuilder(configuration.GetRequiredValue("ConnectionStrings:DefaultConnection"))
        {
            // We rely on error details to get the offending duplicate key values in the TrsDataSyncHelper
            IncludeErrorDetail = true
        }.ConnectionString;

    private class DbDataSourceConnectionFactory(NpgsqlDataSource dataSource) : IConnectionFactory
    {
        public NpgsqlConnection GetOrCreateConnection() => dataSource.CreateConnection();
    }
}
