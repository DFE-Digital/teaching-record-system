using Hangfire;
using Hangfire.PostgreSql;
using QualifiedTeachersApi;
using QualifiedTeachersApi.Jobs.Scheduling;

namespace TeacherIdentity.AuthServer.Services.BackgroundJobs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        string postgresConnectionString)
    {
        if (!environment.IsUnitTests() && !environment.IsEndToEndTests())
        {
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(postgresConnectionString));

            services.AddHangfireServer();

            services.AddSingleton<IHostedService, RegisterRecurringJobsHostedService>();
        }

        return services;
    }
}
