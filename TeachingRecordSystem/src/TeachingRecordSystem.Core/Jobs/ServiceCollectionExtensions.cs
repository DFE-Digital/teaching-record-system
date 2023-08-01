using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("RecurringJobs:Enabled"))
        {
            if ((!environment.IsUnitTests() && !environment.IsEndToEndTests()))
            {
                services.AddOptions<RecurringJobsOptions>()
                    .Bind(configuration.GetSection("RecurringJobs"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                services.AddOptions<BatchSendQtsAwardedEmailsJobOptions>()
                    .Bind(configuration.GetSection("RecurringJobs:BatchSendQtsAwardedEmails"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                services.AddOptions<BatchSendInternationalQtsAwardedEmailsJobOptions>()
                    .Bind(configuration.GetSection("RecurringJobs:BatchSendInternationalQtsAwardedEmails"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                services.AddOptions<BatchSendEytsAwardedEmailsJobOptions>()
                    .Bind(configuration.GetSection("RecurringJobs:BatchSendEytsAwardedEmails"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                services.AddOptions<BatchSendInductionCompletedEmailsJobOptions>()
                    .Bind(configuration.GetSection("RecurringJobs:BatchSendInductionCompletedEmails"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                services.AddSingleton<IHostedService, RegisterRecurringJobsHostedService>();
                services.AddTransient<SendQtsAwardedEmailJob>();
                services.AddTransient<QtsAwardedEmailJobDispatcher>();
                services.AddTransient<SendInternationalQtsAwardedEmailJob>();
                services.AddTransient<InternationalQtsAwardedEmailJobDispatcher>();
                services.AddTransient<SendEytsAwardedEmailJob>();
                services.AddTransient<EytsAwardedEmailJobDispatcher>();
                services.AddTransient<SendInductionCompletedEmailJob>();
                services.AddTransient<InductionCompletedEmailJobDispatcher>();
            }

            if (environment.IsProduction())
            {
                services.AddSingleton<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();
            }
            else
            {
                services.AddSingleton<IBackgroundJobScheduler, ExecuteImmediatelyJobScheduler>();
            }
        }

        return services;
    }
}
