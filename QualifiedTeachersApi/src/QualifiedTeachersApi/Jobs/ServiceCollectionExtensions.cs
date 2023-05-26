using Hangfire;
using Hangfire.PostgreSql;
using QualifiedTeachersApi.Jobs.Scheduling;

namespace QualifiedTeachersApi.Jobs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        string postgresConnectionString)
    {
        if ((!environment.IsUnitTests() && !environment.IsEndToEndTests()) || true)
        {
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(postgresConnectionString));

            services.AddHangfireServer();

            services.AddOptions<RecurringJobsOptions>()
                .Bind(configuration.GetSection("RecurringJobs"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddOptions<BatchSendQtsAwardedEmailsJobOptions>()
                .Bind(configuration.GetSection("RecurringJobs:BatchSendQtsAwardedEmails"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IHostedService, RegisterRecurringJobsHostedService>();
            services.AddTransient<SendQtsAwardedEmailJob>();
        }

        if (environment.IsProduction())
        {
            services.AddSingleton<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();
        }
        else
        {
            services.AddSingleton<IBackgroundJobScheduler, ExecuteImmediatelyJobScheduler>();
        }

        return services;
    }
}
