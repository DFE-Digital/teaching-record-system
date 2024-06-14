using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Establishments.Gias;

namespace TeachingRecordSystem.Core.Jobs;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddBackgroundJobs(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            if (builder.Configuration.GetValue<bool>("RecurringJobs:Enabled"))
            {
                builder.Services.AddOptions<RecurringJobsOptions>()
                    .Bind(builder.Configuration.GetSection("RecurringJobs"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                builder.Services.AddOptions<BatchSendQtsAwardedEmailsJobOptions>()
                    .Bind(builder.Configuration.GetSection("RecurringJobs:BatchSendQtsAwardedEmails"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                builder.Services.AddOptions<BatchSendInternationalQtsAwardedEmailsJobOptions>()
                    .Bind(builder.Configuration.GetSection("RecurringJobs:BatchSendInternationalQtsAwardedEmails"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                builder.Services.AddOptions<BatchSendEytsAwardedEmailsJobOptions>()
                    .Bind(builder.Configuration.GetSection("RecurringJobs:BatchSendEytsAwardedEmails"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                builder.Services.AddOptions<BatchSendInductionCompletedEmailsJobOptions>()
                    .Bind(builder.Configuration.GetSection("RecurringJobs:BatchSendInductionCompletedEmails"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                builder.Services.AddTransient<SendQtsAwardedEmailJob>();
                builder.Services.AddTransient<QtsAwardedEmailJobDispatcher>();
                builder.Services.AddTransient<SendInternationalQtsAwardedEmailJob>();
                builder.Services.AddTransient<InternationalQtsAwardedEmailJobDispatcher>();
                builder.Services.AddTransient<SendEytsAwardedEmailJob>();
                builder.Services.AddTransient<EytsAwardedEmailJobDispatcher>();
                builder.Services.AddTransient<SendInductionCompletedEmailJob>();
                builder.Services.AddTransient<InductionCompletedEmailJobDispatcher>();
                builder.Services.AddHttpClient<PopulateNameSynonymsJob>();

                builder.Services.AddStartupTask(sp =>
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

            builder.Services.AddStartupTask(sp =>
            {
                var recurringJobManager = sp.GetRequiredService<IRecurringJobManager>();

                recurringJobManager.RemoveIfExists("MopUpQtsAwardeesJob");

                recurringJobManager.AddOrUpdate<SyncAllPersonsFromCrmJob>(
                    nameof(SyncAllPersonsFromCrmJob),
                    job => job.Execute(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<SyncAllMqsFromCrmJob>(
                    nameof(SyncAllMqsFromCrmJob),
                    job => job.Execute(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<PopulateNameSynonymsJob>(
                    nameof(PopulateNameSynonymsJob),
                    job => job.Execute(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<PopulateAllPersonsSearchAttributesJob>(
                    nameof(PopulateAllPersonsSearchAttributesJob),
                    job => job.Execute(CancellationToken.None),
                    Cron.Never);

                var giasOptions = sp.GetRequiredService<IOptions<GiasOptions>>();
                recurringJobManager.AddOrUpdate<RefreshEstablishmentsJob>(
                    nameof(RefreshEstablishmentsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    giasOptions!.Value.RefreshEstablishmentsJobSchedule);

                recurringJobManager.AddOrUpdate<ImportTpsCsvExtractFileJob>(
                    nameof(ImportTpsCsvExtractFileJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<TpsRefreshEstablishmentsJob>(
                    nameof(TpsRefreshEstablishmentsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<ProcessEndedEmploymentsJob>(
                    nameof(ProcessEndedEmploymentsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillNinoAndPersonPostcodeInEmploymentHistoryJob>(
                    nameof(BackfillNinoAndPersonPostcodeInEmploymentHistoryJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                return Task.CompletedTask;
            });
        }

        return builder;
    }
}
