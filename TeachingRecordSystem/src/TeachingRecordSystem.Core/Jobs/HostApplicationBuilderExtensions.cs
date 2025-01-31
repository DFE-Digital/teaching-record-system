using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;
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
                builder.Services.AddOptions<InductionStatusUpdatedSupportJobOptions>()
                    .Bind(builder.Configuration.GetSection("RecurringJobs:InductionStatusUpdatedSupportJob"))
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
                builder.Services.AddTransient<InductionStatusUpdatedSupportJob>();

                builder.Services.AddStartupTask(sp =>
                {
                    var recurringJobManager = sp.GetRequiredService<IRecurringJobManager>();
                    var options = sp.GetRequiredService<IOptions<RecurringJobsOptions>>().Value;

                    recurringJobManager.AddOrUpdate<BatchSendQtsAwardedEmailsJob>(
                        nameof(BatchSendQtsAwardedEmailsJob),
                        job => job.ExecuteAsync(CancellationToken.None),
                        options.BatchSendQtsAwardedEmails.JobSchedule);

                    recurringJobManager.AddOrUpdate<BatchSendInternationalQtsAwardedEmailsJob>(
                        nameof(BatchSendInternationalQtsAwardedEmailsJob),
                        job => job.ExecuteAsync(CancellationToken.None),
                        options.BatchSendInternationalQtsAwardedEmails.JobSchedule);

                    recurringJobManager.AddOrUpdate<BatchSendEytsAwardedEmailsJob>(
                        nameof(BatchSendEytsAwardedEmailsJob),
                        job => job.ExecuteAsync(CancellationToken.None),
                        options.BatchSendEytsAwardedEmails.JobSchedule);

                    recurringJobManager.AddOrUpdate<BatchSendInductionCompletedEmailsJob>(
                        nameof(BatchSendInductionCompletedEmailsJob),
                        job => job.ExecuteAsync(CancellationToken.None),
                        options.BatchSendInductionCompletedEmails.JobSchedule);

                    return Task.CompletedTask;
                });
            }

            builder.Services.AddTransient<EwcWalesImportJob>();
            builder.Services.AddTransient<QtsImporter>();
            builder.Services.AddTransient<InductionImporter>();

            builder.Services.AddStartupTask(sp =>
            {
                var recurringJobManager = sp.GetRequiredService<IRecurringJobManager>();

                recurringJobManager.RemoveIfExists("MopUpQtsAwardeesJob");
                recurringJobManager.RemoveIfExists("SyncAllMqsFromCrmJob");

                recurringJobManager.AddOrUpdate<SyncAllPersonsFromCrmJob>(
                    nameof(SyncAllPersonsFromCrmJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<PopulateNameSynonymsJob>(
                    nameof(PopulateNameSynonymsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<PopulateAllPersonsSearchAttributesJob>(
                    nameof(PopulateAllPersonsSearchAttributesJob),
                    job => job.ExecuteAsync(CancellationToken.None),
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

                recurringJobManager.AddOrUpdate<DeleteOldAttachmentsJob>(
                    nameof(DeleteOldAttachmentsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    DeleteOldAttachmentsJob.JobSchedule);

                recurringJobManager.AddOrUpdate<BackfillDqtReportingQualifications>(
                    nameof(BackfillDqtReportingQualifications),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillDqtReportingWorkforceData>(
                    nameof(BackfillDqtReportingWorkforceData),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillDqtReportingPersons>(
                    nameof(BackfillDqtReportingPersons),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<ExportWorkforceDataJob>(
                    nameof(ExportWorkforceDataJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<InductionStatusUpdatedSupportJob>(
                    nameof(InductionStatusUpdatedSupportJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    InductionStatusUpdatedSupportJob.JobSchedule);

                recurringJobManager.RemoveIfExists("SyncAllAlertsFromCrmJob");
                recurringJobManager.RemoveIfExists("SyncAllAlertsFromCrmJob (dry-run)");
                recurringJobManager.RemoveIfExists("SyncAllAlertsFromCrmJob & migrate");

                recurringJobManager.AddOrUpdate<SyncAllInductionsFromCrmJob>(
                    nameof(SyncAllInductionsFromCrmJob),
                    job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<SyncAllInductionsFromCrmJob>(
                    $"{nameof(SyncAllInductionsFromCrmJob)} & (dry-run)",
                    job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                    Cron.Never);

                recurringJobManager.RemoveIfExists($"{nameof(SyncAllInductionsFromCrmJob)} & migrate");

                recurringJobManager.AddOrUpdate<BackfillDqtInductionEventEnumDescriptionsJob>(
                    nameof(BackfillDqtInductionEventEnumDescriptionsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<ClearAlertsJob>(
                    nameof(ClearAlertsJob),
                    job => job.ExecuteAsync(),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillDqtReportingAlertTypes>(
                    nameof(BackfillDqtReportingAlertTypes),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<SyncAllDqtContactAuditsJob>(
                    nameof(SyncAllDqtContactAuditsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<SyncAllDqtInductionAuditsJob>(
                    nameof(SyncAllDqtInductionAuditsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<SyncAllDqtIttAuditsJob>(
                    nameof(SyncAllDqtIttAuditsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<SyncAllDqtQtsAuditsJob>(
                    nameof(SyncAllDqtQtsAuditsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<SyncDqtContactAuditsMopUpJob>(
                    nameof(SyncDqtContactAuditsMopUpJob),
                    job => job.ExecuteAsync(/*modifiedSince: */new DateTime(2024, 12, 24), CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<EwcWalesImportJob>(
                    nameof(EwcWalesImportJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    EwcWalesImportJob.JobSchedule);

                recurringJobManager.AddOrUpdate<MigrateInductionsFromCrmJob>(
                    nameof(MigrateInductionsFromCrmJob),
                    job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<MigrateInductionsFromCrmJob>(
                    $"{nameof(MigrateInductionsFromCrmJob)} & (dry-run)",
                    job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                    Cron.Never);

                return Task.CompletedTask;
            });
        }

        return builder;
    }
}
