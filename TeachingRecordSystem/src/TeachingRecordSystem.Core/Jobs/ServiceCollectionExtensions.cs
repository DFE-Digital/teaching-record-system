using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        if ((!environment.IsUnitTests() && !environment.IsEndToEndTests()))
        {
            if (configuration.GetValue<bool>("RecurringJobs:Enabled"))
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

                services.AddTransient<SendQtsAwardedEmailJob>();
                services.AddTransient<QtsAwardedEmailJobDispatcher>();
                services.AddTransient<SendInternationalQtsAwardedEmailJob>();
                services.AddTransient<InternationalQtsAwardedEmailJobDispatcher>();
                services.AddTransient<SendEytsAwardedEmailJob>();
                services.AddTransient<EytsAwardedEmailJobDispatcher>();
                services.AddTransient<SendInductionCompletedEmailJob>();
                services.AddTransient<InductionCompletedEmailJobDispatcher>();

                services.AddStartupTask(sp =>
                {
                    var recurringJobManager = sp.GetRequiredService<IRecurringJobManager>();
                    var options = sp.GetRequiredService<IOptions<RecurringJobsOptions>>().Value;

                    recurringJobManager.AddOrUpdate<BatchSendQtsAwardedEmailsJob>(
                        nameof(BatchSendQtsAwardedEmailsJob),
                        job => job.Execute(CancellationToken.None),
                        options.BatchSendQtsAwardedEmails.JobSchedule);

                    recurringJobManager.AddOrUpdate<BatchSendInternationalQtsAwardedEmailsJob>(
                        nameof(BatchSendInternationalQtsAwardedEmailsJob),
                        job => job.Execute(CancellationToken.None),
                        options.BatchSendInternationalQtsAwardedEmails.JobSchedule);

                    recurringJobManager.AddOrUpdate<BatchSendEytsAwardedEmailsJob>(
                        nameof(BatchSendEytsAwardedEmailsJob),
                        job => job.Execute(CancellationToken.None),
                        options.BatchSendEytsAwardedEmails.JobSchedule);

                    recurringJobManager.AddOrUpdate<BatchSendInductionCompletedEmailsJob>(
                        nameof(BatchSendInductionCompletedEmailsJob),
                        job => job.Execute(CancellationToken.None),
                        options.BatchSendInductionCompletedEmails.JobSchedule);

                    return Task.CompletedTask;
                });
            }
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
