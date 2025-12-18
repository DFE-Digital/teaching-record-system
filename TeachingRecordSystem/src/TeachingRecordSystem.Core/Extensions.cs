using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;
using Serilog.Formatting.Compact;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.Notes;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core;

public static class Extensions
{
    public static IConfigurationBuilder AddAksConfiguration(this IConfigurationBuilder builder)
    {
        var deployedEnvironmentName = Environment.GetEnvironmentVariable("ENVIRONMENT_NAME")
            ?? throw new InvalidOperationException("ENVIRONMENT_NAME environment variable is not set.");

        return builder
            .AddJsonFile($"appsettings.aks_{deployedEnvironmentName}.json")
            .AddJsonFile($"appsettings.aks_{deployedEnvironmentName}_shared.json");
    }

    public static IServiceCollection AddBackgroundJobScheduler(this IServiceCollection services, IHostEnvironment environment)
    {
        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
            services.AddSingleton<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();
        }

        return services;
    }

    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(configuration.GetRequiredValue("StorageConnectionString"));
            });

            services.AddKeyedSingleton<DataLakeServiceClient>("sftpstorage", (sp, key) =>
            {
                var sftpAccountName = configuration.GetValue<string>("SftpStorageName");
                var sftpAccessKey = configuration.GetValue<string>("SftpStorageAccessKey");

                if (string.IsNullOrEmpty(sftpAccountName) || string.IsNullOrEmpty(sftpAccessKey))
                {
                    throw new InvalidOperationException("Invalid SFTP Storage connection string configuration.");
                }

                var dfsUri = new Uri($"https://{sftpAccountName}.dfs.core.windows.net");
                var credential = new StorageSharedKeyCredential(sftpAccountName, sftpAccessKey);

                return new DataLakeServiceClient(dfsUri, credential);
            });
        }

        return services;
    }

    public static IHostApplicationBuilder AddCoreServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddCoreServices(builder.Configuration, builder.Environment);

        return builder;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
            services.AddOptions<AccessYourTeachingQualificationsOptions>()
                .Bind(configuration.GetSection("AccessYourTeachingQualifications"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        services
            .AddClock()
            .AddSingleton<IFeatureProvider, ConfigurationFeatureProvider>()
            .AddDatabase(configuration)
            .AddHangfire(environment)
            .AddBackgroundJobScheduler(environment)
            .AddWebhookMessageFactory()
            .AddSingleton<ReferenceDataCache>()
            .AddBlobStorage(configuration, environment)
            .AddFileService()
            .AddNameSynonyms()
            .AddTrnRequestService(configuration)
            .AddEventPublisher()
            .AddSupportTaskServices()
            .AddSingleton<PersonInfoCache>()
            .AddNoteService()
            .AddPersonService()
            .AddOneLoginService();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetPostgresConnectionString();

        return services.AddDatabase(connectionString);
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

    public static IServiceCollection AddClock(this IServiceCollection services)
    {
        services.AddSingleton<IClock, Clock>();

        return services;
    }

    public static IServiceCollection AddEventPublisher(this IServiceCollection services)
    {
        services.AddScoped<IEventPublisher, EventPublisher>();

        services.Scan(s => s.FromAssemblyOf<IEvent>()
            .AddClasses(c => c.AssignableTo<IEventHandler>())
            .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }

    public static IHostApplicationBuilder AddHangfire(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHangfire(builder.Environment);

        return builder;
    }

    public static IServiceCollection AddHangfire(this IServiceCollection services, IHostEnvironment environment)
    {
        var prepareSchemaIfNecessary = true;

        var schemaPreparedMarkerFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TeachingRecordSystem",
            "hangfire-schema-prepared");

        // Try to skip schema preparation in development to speed up startup
        if (environment.IsDevelopment() && File.Exists(schemaPreparedMarkerFile))
        {
            prepareSchemaIfNecessary = false;
        }

        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
            services.AddHangfire((sp, configuration) => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o => o.UseConnectionFactory(new DbDataSourceConnectionFactory(sp.GetRequiredService<NpgsqlDataSource>())),
                    new PostgreSqlStorageOptions()
                    {
                        PrepareSchemaIfNecessary = prepareSchemaIfNecessary,
                        UseSlidingInvisibilityTimeout = true
                    }));
        }

        if (environment.IsDevelopment() && !File.Exists(schemaPreparedMarkerFile))
        {
            Directory.CreateDirectory(Directory.GetParent(schemaPreparedMarkerFile)!.FullName);
            File.WriteAllText(schemaPreparedMarkerFile, string.Empty);
        }

        return services;
    }

    public static IServiceCollection AddWebhookMessageFactory(this IServiceCollection services)
    {
        services
            .AddSingleton<WebhookMessageFactory>()
            .AddSingleton<EventMapperRegistry>();

        return services;
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
        configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");

    private class DbDataSourceConnectionFactory(NpgsqlDataSource dataSource) : IConnectionFactory
    {
        public NpgsqlConnection GetOrCreateConnection() => dataSource.CreateConnection();
    }
}
