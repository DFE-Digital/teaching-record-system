using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;
using TeachingRecordSystem.Core.Services.Establishments.Gias;
using TeachingRecordSystem.Core.Services.PublishApi;

namespace TeachingRecordSystem.Core.Jobs;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddBackgroundJobs(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            if (builder.Configuration.GetValue<bool>("RecurringJobsEnabled"))
            {
                builder.Services.AddOptions<BatchSendProfessionalStatusEmailsOptions>()
                    .Bind(builder.Configuration.GetSection("BatchSendProfessionalStatusEmailsJob"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                builder.Services.AddOptions<BatchSendInductionCompletedEmailsJobOptions>()
                    .Bind(builder.Configuration.GetSection("BatchSendInductionCompletedEmailsJob"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                builder.Services.AddOptions<InductionStatusUpdatedSupportJobOptions>()
                    .Bind(builder.Configuration.GetSection("RecurringJobs:InductionStatusUpdatedSupportJob"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                builder.Services.AddOptions<CapitaTpsUserOption>()
                    .Bind(builder.Configuration.GetSection("RecurringJobs:CapitaTpsImport"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                builder.Services.AddStartupTask(sp =>
                {
                    var recurringJobManager = sp.GetRequiredService<IRecurringJobManager>();

                    var professionalStatusEmailJobOptions = sp.GetRequiredService<IOptions<BatchSendProfessionalStatusEmailsOptions>>().Value;
                    recurringJobManager.AddOrUpdate<BatchSendProfessionalStatusEmailsJob>(
                        nameof(BatchSendProfessionalStatusEmailsJob),
                        job => job.ExecuteAsync(CancellationToken.None),
                        professionalStatusEmailJobOptions.JobSchedule);

                    var inductionEmailJobOptions = sp.GetRequiredService<IOptions<BatchSendInductionCompletedEmailsJobOptions>>().Value;
                    recurringJobManager.AddOrUpdate<BatchSendInductionCompletedEmailsJob>(
                        nameof(BatchSendInductionCompletedEmailsJob),
                        job => job.ExecuteAsync(CancellationToken.None),
                        inductionEmailJobOptions.JobSchedule);

                    return Task.CompletedTask;
                });
            }

            builder.Services.AddHttpClient<PopulateNameSynonymsJob>();

            builder.Services.AddTransient<QtsImporter>();
            builder.Services.AddTransient<InductionImporter>();

            builder.Services.AddStartupTask(sp =>
            {
                var recurringJobManager = sp.GetRequiredService<IRecurringJobManager>();

                recurringJobManager.RemoveIfExists("MopUpQtsAwardeesJob");
                recurringJobManager.RemoveIfExists("SyncAllMqsFromCrmJob");
                recurringJobManager.RemoveIfExists("BackfillNinoAndPersonPostcodeInEmploymentHistoryJob");
                recurringJobManager.RemoveIfExists("BatchSendQtsAwardedEmailsJob");
                recurringJobManager.RemoveIfExists("BatchSendInternationalQtsAwardedEmailsJob");
                recurringJobManager.RemoveIfExists("BatchSendEytsAwardedEmailsJob");

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
                    giasOptions.Value.RefreshEstablishmentsJobSchedule);

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

                recurringJobManager.RemoveIfExists("DeleteOldAttachmentsJob");

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

                recurringJobManager.AddOrUpdate<BackfillDqtNotesJob>(
                    nameof(BackfillDqtNotesJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.RemoveIfExists("SyncAllAlertsFromCrmJob");
                recurringJobManager.RemoveIfExists("SyncAllAlertsFromCrmJob (dry-run)");
                recurringJobManager.RemoveIfExists("SyncAllAlertsFromCrmJob & migrate");

                recurringJobManager.RemoveIfExists("SyncAllInductionsFromCrmJob");
                recurringJobManager.RemoveIfExists("SyncAllInductionsFromCrmJob & (dry-run)");
                recurringJobManager.RemoveIfExists("SyncAllInductionsFromCrmJob & migrate");

                recurringJobManager.RemoveIfExists("MigrateRoutesFromCrmJob");
                recurringJobManager.RemoveIfExists("MigrateRoutesFromCrmJob (dry-run)");

                recurringJobManager.AddOrUpdate<CpdInductionImporterJob>(
                    nameof(CpdInductionImporterJob),
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

                var publishApiOptions = sp.GetRequiredService<IOptions<PublishApiOptions>>().Value;
                recurringJobManager.AddOrUpdate<RefreshTrainingProvidersJob>(
                    nameof(RefreshTrainingProvidersJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    publishApiOptions.RefreshTrainingProvidersJobSchedule);

                recurringJobManager.AddOrUpdate<BackfillEmployerEmailAddressInEmploymentHistoryJob>(
                    nameof(BackfillEmployerEmailAddressInEmploymentHistoryJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<AppendTrainingProvidersFromCrmJob>(
                    nameof(AppendTrainingProvidersFromCrmJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<ResyncAllPersonsJob>(
                    nameof(ResyncAllPersonsJob),
                    job => job.ExecuteAsync(new DateTime(2025, 8, 16, 0, 0, 0, DateTimeKind.Utc), CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<FixIncorrectOttRouteMigrationMappingsJob>(
                    nameof(FixIncorrectOttRouteMigrationMappingsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillDqtIttQtsBusinessEventAuditsJob>(
                    $"{nameof(BackfillDqtIttQtsBusinessEventAuditsJob)} (dry-run)",
                    job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillDqtIttQtsBusinessEventAuditsJob>(
                    nameof(BackfillDqtIttQtsBusinessEventAuditsJob),
                    job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<SyncAllPreviousNamesFromCrmJob>(
                    nameof(SyncAllPreviousNamesFromCrmJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillPersonCreatedByTpsJob>(
                    nameof(BackfillPersonCreatedByTpsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<CreatePersonMigratedEventsJob>(
                    nameof(CreatePersonMigratedEventsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<CapitaExportNewJob>(
                    nameof(CapitaExportNewJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    CapitaExportNewJob.JobSchedule);

                recurringJobManager.AddOrUpdate<CapitaExportAmendJob>(
                    nameof(CapitaExportAmendJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    CapitaExportAmendJob.JobSchedule);

                return Task.CompletedTask;
            });
        }

        return builder;
    }
}
