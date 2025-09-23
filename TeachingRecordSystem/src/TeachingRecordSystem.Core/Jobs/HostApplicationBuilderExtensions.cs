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
        if (!builder.Environment.IsTests() && !builder.Environment.IsEndToEndTests())
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

            builder.Services.AddOptions<CapitaTpsUserOption>()
                .BindConfiguration("RecurringJobs:CapitaTpsImport")
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddOptions<DeletePersonAndChildRecordsWithoutATrnOptions>()
                .BindConfiguration("RecurringJobs:DeletePersonAndChildRecordsWithoutATrn")
                .ValidateDataAnnotations()
                .ValidateOnStart();

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
                recurringJobManager.RemoveIfExists("DeleteOldAttachmentsJob");

                recurringJobManager.RemoveIfExists("SyncAllAlertsFromCrmJob");
                recurringJobManager.RemoveIfExists("SyncAllAlertsFromCrmJob (dry-run)");
                recurringJobManager.RemoveIfExists("SyncAllAlertsFromCrmJob & migrate");

                recurringJobManager.RemoveIfExists("SyncAllInductionsFromCrmJob");
                recurringJobManager.RemoveIfExists("SyncAllInductionsFromCrmJob & (dry-run)");
                recurringJobManager.RemoveIfExists("SyncAllInductionsFromCrmJob & migrate");

                recurringJobManager.RemoveIfExists("MigrateRoutesFromCrmJob");
                recurringJobManager.RemoveIfExists("MigrateRoutesFromCrmJob (dry-run)");

                recurringJobManager.RemoveIfExists("ResetIncorrectHasEypsOnPersonsJob (dry-run)");
                recurringJobManager.RemoveIfExists("ResetIncorrectHasEypsOnPersonsJob");

                recurringJobManager.RemoveIfExists("SetMissingHasEypsOnPersonsJob (dry-run)");
                recurringJobManager.RemoveIfExists("SetMissingHasEypsOnPersonsJob");

                recurringJobManager.RemoveIfExists("AllocateTrnsToPersonsWithEyps (dry-run)");
                recurringJobManager.RemoveIfExists("AllocateTrnsToPersonsWithEyps");

                recurringJobManager.RemoveIfExists("BackfillDqtReportingAlertTypes");
                recurringJobManager.RemoveIfExists("BackfillDqtReportingPersons");
                recurringJobManager.RemoveIfExists("BackfillDqtReportingQualifications");
                recurringJobManager.RemoveIfExists("BackfillDqtReportingWorkforceData");

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

                recurringJobManager.AddOrUpdate<BackfillDqtReportingQualificationsJob>(
                    nameof(BackfillDqtReportingQualificationsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillDqtReportingWorkforceDataJob>(
                    nameof(BackfillDqtReportingWorkforceDataJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillDqtReportingPersonsJob>(
                    nameof(BackfillDqtReportingPersonsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<ExportWorkforceDataJob>(
                    nameof(ExportWorkforceDataJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.RemoveIfExists("InductionStatusUpdatedSupportJob");

                recurringJobManager.RemoveIfExists("BackfillDqtNotesJob");

                recurringJobManager.AddOrUpdate<CpdInductionImporterJob>(
                    nameof(CpdInductionImporterJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<ClearAlertsJob>(
                    nameof(ClearAlertsJob),
                    job => job.ExecuteAsync(),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<BackfillDqtReportingAlertTypesJob>(
                    nameof(BackfillDqtReportingAlertTypesJob),
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

                recurringJobManager.RemoveIfExists("AppendTrainingProvidersFromCrmJob");

                recurringJobManager.AddOrUpdate<ResyncAllPersonsJob>(
                    nameof(ResyncAllPersonsJob),
                    job => job.ExecuteAsync(new DateTime(2025, 8, 22, 0, 0, 0, DateTimeKind.Utc), CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<FixIncorrectOttRouteMigrationMappingsJob>(
                    nameof(FixIncorrectOttRouteMigrationMappingsJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                recurringJobManager.RemoveIfExists("BackfillDqtIttQtsBusinessEventAuditsJob (dry-run)");
                recurringJobManager.RemoveIfExists("BackfillDqtIttQtsBusinessEventAuditsJob");
                recurringJobManager.RemoveIfExists("SyncAllPreviousNamesFromCrmJob");

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

                recurringJobManager.AddOrUpdate<AllocateTrnToPersonJob>(
                    $"{nameof(AllocateTrnToPersonJob)} (dry-run)",
                    job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<AllocateTrnToPersonJob>(
                    nameof(AllocateTrnToPersonJob),
                    job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<CapitaImportJob>(
                    nameof(CapitaImportJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    CapitaImportJob.JobSchedule);

                recurringJobManager.AddOrUpdate<DeletePersonAndChildRecordsWithoutATrnJob>(
                    $"{nameof(DeletePersonAndChildRecordsWithoutATrnJob)} (dry-run)",
                    job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<DeletePersonAndChildRecordsWithoutATrnJob>(
                    nameof(DeletePersonAndChildRecordsWithoutATrnJob),
                    job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                    Cron.Never);

                recurringJobManager.AddOrUpdate<DeleteStaleJourneyStatesJob>(
                    nameof(DeleteStaleJourneyStatesJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    DeleteStaleJourneyStatesJob.JobSchedule);

                recurringJobManager.AddOrUpdate<BackfillPersonAttributesJob>(
                    nameof(BackfillPersonAttributesJob),
                    job => job.ExecuteAsync(CancellationToken.None),
                    Cron.Never);

                return Task.CompletedTask;
            });
        }

        return builder;
    }
}
