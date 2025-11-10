using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;
using TeachingRecordSystem.Core.Services.Establishments.Gias;
using TeachingRecordSystem.Core.Services.PublishApi;

namespace TeachingRecordSystem.Core.Jobs;

public static class Extensions
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddHttpClient<PopulateNameSynonymsJob>();
        services.AddTransient<QtsImporter>();
        services.AddTransient<InductionImporter>();

        services.AddOptions<BatchSendProfessionalStatusEmailsOptions>()
            .Bind(configuration.GetSection("BatchSendProfessionalStatusEmailsJob"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<BatchSendInductionCompletedEmailsJobOptions>()
            .Bind(configuration.GetSection("BatchSendInductionCompletedEmailsJob"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<InductionStatusUpdatedSupportJobOptions>()
            .Bind(configuration.GetSection("RecurringJobs:InductionStatusUpdatedSupportJob"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CapitaTpsUserOption>()
            .BindConfiguration("RecurringJobs:CapitaTpsImport")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<DeletePersonAndChildRecordsWithoutATrnOptions>()
            .BindConfiguration("RecurringJobs:DeletePersonAndChildRecordsWithoutATrn")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        string GetRecurringJobSchedule(string cronExpression) =>
            configuration.GetValue<bool>("RecurringJobsEnabled") && environment.IsProduction() ? cronExpression : Cron.Never();

        services.AddStartupTask(sp =>
        {
            var recurringJobManager = sp.GetRequiredService<IRecurringJobManager>();

            var professionalStatusEmailJobOptions = sp.GetRequiredService<IOptions<BatchSendProfessionalStatusEmailsOptions>>().Value;
            recurringJobManager.AddOrUpdate<BatchSendProfessionalStatusEmailsJob>(
                nameof(BatchSendProfessionalStatusEmailsJob),
                job => job.ExecuteAsync(CancellationToken.None),
                GetRecurringJobSchedule(professionalStatusEmailJobOptions.JobSchedule));

            var inductionEmailJobOptions = sp.GetRequiredService<IOptions<BatchSendInductionCompletedEmailsJobOptions>>().Value;
            recurringJobManager.AddOrUpdate<BatchSendInductionCompletedEmailsJob>(
                nameof(BatchSendInductionCompletedEmailsJob),
                job => job.ExecuteAsync(CancellationToken.None),
                GetRecurringJobSchedule(inductionEmailJobOptions.JobSchedule));

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
                GetRecurringJobSchedule(giasOptions.Value.RefreshEstablishmentsJobSchedule));

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
                job => job.ExecuteAsync(/*performContext: */null!, CancellationToken.None),
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
                GetRecurringJobSchedule(EwcWalesImportJob.JobSchedule));

            var publishApiOptions = sp.GetRequiredService<IOptions<PublishApiOptions>>().Value;
            recurringJobManager.AddOrUpdate<RefreshTrainingProvidersJob>(
                nameof(RefreshTrainingProvidersJob),
                job => job.ExecuteAsync(CancellationToken.None),
                publishApiOptions.RefreshTrainingProvidersJobSchedule);

            recurringJobManager.AddOrUpdate<BackfillEmployerEmailAddressInEmploymentHistoryJob>(
                nameof(BackfillEmployerEmailAddressInEmploymentHistoryJob),
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<ResyncAllPersonsJob>(
                nameof(ResyncAllPersonsJob),
                job => job.ExecuteAsync(new DateTime(2025, 8, 22, 0, 0, 0, DateTimeKind.Utc), CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<FixIncorrectOttRouteMigrationMappingsJob>(
                nameof(FixIncorrectOttRouteMigrationMappingsJob),
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
                GetRecurringJobSchedule(CapitaExportNewJob.JobSchedule));

            recurringJobManager.AddOrUpdate<CapitaExportAmendJob>(
                nameof(CapitaExportAmendJob),
                job => job.ExecuteAsync(CancellationToken.None),
                GetRecurringJobSchedule(CapitaExportAmendJob.JobSchedule));

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
                GetRecurringJobSchedule(CapitaImportJob.JobSchedule));

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
                GetRecurringJobSchedule(DeleteStaleJourneyStatesJob.JobSchedule));

            recurringJobManager.AddOrUpdate<BackfillPersonAttributesJob>(
                nameof(BackfillPersonAttributesJob),
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillDqtReportingSupportTasksJob>(
                nameof(BackfillDqtReportingSupportTasksJob),
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<AllocateTrnsToOverseasNpqApplicantsJob>(
                nameof(AllocateTrnsToOverseasNpqApplicantsJob),
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<SyncAllDqtAnnotationAuditsJob>(
                nameof(SyncAllDqtAnnotationAuditsJob),
                job => job.ExecuteAsync(/*performContext: */null!, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillCapitaImportWarningStatusesJob>(
                $"{nameof(BackfillCapitaImportWarningStatusesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillCapitaImportWarningStatusesJob>(
                nameof(BackfillCapitaImportWarningStatusesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillTrnRequestMetadataInReportingDb>(
                nameof(BackfillTrnRequestMetadataInReportingDb),
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillUsersInReportingDb>(
                nameof(BackfillUsersInReportingDb),
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillSupportTasksInReportingDb>(
                nameof(BackfillSupportTasksInReportingDb),
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Never);

            return Task.CompletedTask;
        });

        return services;
    }
}
