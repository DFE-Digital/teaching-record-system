using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;
using Serilog.Formatting.Compact;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core;

public static class Extensions
{
    public static IHostApplicationBuilder AddBackgroundWorkScheduler(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddSingleton<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();
        }
        else
        {
            builder.Services.AddSingleton<IBackgroundJobScheduler, ExecuteImmediatelyJobScheduler>();
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
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<TrsDbContext>(options => TrsDbContext.ConfigureOptions(options));

        return services;
    }

    public static IHostApplicationBuilder AddHangfire(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            var pgConnectionString = GetPostgresConnectionString(builder.Configuration);

            builder.Services.AddHangfire((sp, configuration) => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o => o.UseExistingNpgsqlConnection(sp.GetRequiredService<NpgsqlDataSource>().CreateConnection())));
        }

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
            .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces)
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
}
