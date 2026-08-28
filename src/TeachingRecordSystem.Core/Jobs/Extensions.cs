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

        services.AddOptions<ScheduleTrnRecipientEmailsJobOptions>()
            .Bind(configuration.GetSection("ScheduleTrnRecipientEmailsJob"))
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

        services.AddOptions<DeleteOldEvidenceFilesJobOptions>()
            .Bind(configuration.GetSection("DeleteOldEvidenceFilesJob"))
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

            recurringJobManager.RemoveIfExists("BackfillDqtReportingQualificationsJob");

            recurringJobManager.RemoveIfExists("BackfillDqtReportingWorkforceDataJob");

            recurringJobManager.RemoveIfExists("BackfillDqtReportingPersonsJob");

            recurringJobManager.RemoveIfExists("ExportWorkforceDataJob");

            recurringJobManager.RemoveIfExists("BackfillDqtReportingAlertTypesJob");

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

            recurringJobManager.AddOrUpdate<CapitaExportNewJob>(
                nameof(CapitaExportNewJob),
                job => job.ExecuteAsync(CancellationToken.None),
                GetRecurringJobSchedule(CapitaExportNewJob.JobSchedule));

            recurringJobManager.AddOrUpdate<CapitaExportAmendJob>(
                nameof(CapitaExportAmendJob),
                job => job.ExecuteAsync(CancellationToken.None),
                GetRecurringJobSchedule(CapitaExportAmendJob.JobSchedule));

            recurringJobManager.RemoveIfExists("AllocateTrnToPersonJob (dry-run)");

            recurringJobManager.RemoveIfExists("AllocateTrnToPersonJob");

            recurringJobManager.AddOrUpdate<CapitaImportJob>(
                nameof(CapitaImportJob),
                job => job.ExecuteAsync(CancellationToken.None),
                GetRecurringJobSchedule(CapitaImportJob.JobSchedule));

            recurringJobManager.RemoveIfExists("DeletePersonAndChildRecordsWithoutATrnJob (dry-run)");
            recurringJobManager.RemoveIfExists("DeletePersonAndChildRecordsWithoutATrnJob");

            recurringJobManager.RemoveIfExists("DeleteStaleJourneyStatesJob");

            recurringJobManager.AddOrUpdate<BackfillPersonAttributesJob>(
                nameof(BackfillPersonAttributesJob),
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Never);

            recurringJobManager.RemoveIfExists("BackfillDqtReportingSupportTasksJob");

            recurringJobManager.RemoveIfExists("AllocateTrnsToOverseasNpqApplicantsJob");

            recurringJobManager.AddOrUpdate<BackfillCapitaImportWarningStatusesJob>(
                $"{nameof(BackfillCapitaImportWarningStatusesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillCapitaImportWarningStatusesJob>(
                nameof(BackfillCapitaImportWarningStatusesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.RemoveIfExists("BackfillTrnRequestMetadataInReportingDb");

            recurringJobManager.AddOrUpdate<BackfillNormalizePersonNamesJob>(
                $"{nameof(BackfillNormalizePersonNamesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillNormalizePersonNamesJob>(
                nameof(BackfillNormalizePersonNamesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.RemoveIfExists("BackfillUsersInReportingDb");

            recurringJobManager.AddOrUpdate<BackfillUserProcessesJob>(
                $"{nameof(BackfillUserProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillUserProcessesJob>(
                nameof(BackfillUserProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillApiKeyProcessesJob>(
                $"{nameof(BackfillApiKeyProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillApiKeyProcessesJob>(
                nameof(BackfillApiKeyProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillMandatoryQualificationProcessesJob>(
                $"{nameof(BackfillMandatoryQualificationProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillMandatoryQualificationProcessesJob>(
                nameof(BackfillMandatoryQualificationProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillChangeRequestProcessesJob>(
                $"{nameof(BackfillChangeRequestProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillChangeRequestProcessesJob>(
                nameof(BackfillChangeRequestProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillChangeRequestApprovalProcessesJob>(
                $"{nameof(BackfillChangeRequestApprovalProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillChangeRequestApprovalProcessesJob>(
                nameof(BackfillChangeRequestApprovalProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillChangeRequestEmailSentEventsJob>(
                $"{nameof(BackfillChangeRequestEmailSentEventsJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillChangeRequestEmailSentEventsJob>(
                nameof(BackfillChangeRequestEmailSentEventsJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillTeacherPensionsSupportTaskProcessesJob>(
                $"{nameof(BackfillTeacherPensionsSupportTaskProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillTeacherPensionsSupportTaskProcessesJob>(
                nameof(BackfillTeacherPensionsSupportTaskProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillWebhookEndpointProcessesJob>(
                $"{nameof(BackfillWebhookEndpointProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillWebhookEndpointProcessesJob>(
                nameof(BackfillWebhookEndpointProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillAlertDqtProcessesJob>(
                $"{nameof(BackfillAlertDqtProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillAlertDqtProcessesJob>(
                nameof(BackfillAlertDqtProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillDqtInductionProcessesJob>(
                $"{nameof(BackfillDqtInductionProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillDqtInductionProcessesJob>(
                nameof(BackfillDqtInductionProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillDqtInitialTeacherTrainingProcessesJob>(
                $"{nameof(BackfillDqtInitialTeacherTrainingProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillDqtInitialTeacherTrainingProcessesJob>(
                nameof(BackfillDqtInitialTeacherTrainingProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillDqtQtsRegistrationProcessesJob>(
                $"{nameof(BackfillDqtQtsRegistrationProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillDqtQtsRegistrationProcessesJob>(
                nameof(BackfillDqtQtsRegistrationProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillMandatoryQualificationDqtProcessesJob>(
                $"{nameof(BackfillMandatoryQualificationDqtProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillMandatoryQualificationDqtProcessesJob>(
                nameof(BackfillMandatoryQualificationDqtProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillRouteToProfessionalStatusProcessesJob>(
                $"{nameof(BackfillRouteToProfessionalStatusProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillRouteToProfessionalStatusProcessesJob>(
                nameof(BackfillRouteToProfessionalStatusProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillPersonInductionProcessesJob>(
                $"{nameof(BackfillPersonInductionProcessesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillPersonInductionProcessesJob>(
                nameof(BackfillPersonInductionProcessesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.RemoveIfExists("BackfillSupportTasksInReportingDb");

            recurringJobManager.RemoveIfExists("BackfillSupportTaskColumnsJob (dry-run)");
            recurringJobManager.RemoveIfExists("BackfillSupportTaskColumnsJob");

            recurringJobManager.AddOrUpdate<BackfillResolvedAttributesJob>(
                $"{nameof(BackfillResolvedAttributesJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillResolvedAttributesJob>(
                nameof(BackfillResolvedAttributesJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillSupportTaskOutcomeJob>(
                $"{nameof(BackfillSupportTaskOutcomeJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillSupportTaskOutcomeJob>(
                nameof(BackfillSupportTaskOutcomeJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillSupportTaskSourceApplicationUserJob>(
                $"{nameof(BackfillSupportTaskSourceApplicationUserJob)} (dry-run)",
                job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<BackfillSupportTaskSourceApplicationUserJob>(
                nameof(BackfillSupportTaskSourceApplicationUserJob),
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
                Cron.Never);

            recurringJobManager.AddOrUpdate<ScheduleTrnRecipientEmailsJob>(
                nameof(ScheduleTrnRecipientEmailsJob),
                job => job.ExecuteAsync(CancellationToken.None),
                GetRecurringJobSchedule(professionalStatusEmailJobOptions.JobSchedule));

            recurringJobManager.RemoveIfExists("BackfillAlertsInReportingDb");

            recurringJobManager.RemoveIfExists("BackfillAuthzRegistrationTokenJob");

            var deleteOldEvidenceFilesJobOptions = sp.GetRequiredService<IOptions<DeleteOldEvidenceFilesJobOptions>>().Value;
            recurringJobManager.AddOrUpdate<DeleteOldEvidenceFilesJob>(
                nameof(DeleteOldEvidenceFilesJob),
                job => job.ExecuteAsync(CancellationToken.None),
                GetRecurringJobSchedule(deleteOldEvidenceFilesJobOptions.JobSchedule));

            recurringJobManager.RemoveIfExists("BackfillAlertProcessesJob (dry-run)");
            recurringJobManager.RemoveIfExists("BackfillAlertProcessesJob");
            recurringJobManager.RemoveIfExists("CreateDqtAnnotationAuditEventsJob");
            recurringJobManager.RemoveIfExists("SyncAllPersonsFromCrmJob");
            recurringJobManager.RemoveIfExists("ClearAlertsJob");
            recurringJobManager.RemoveIfExists("SyncAllDqtContactAuditsJob");
            recurringJobManager.RemoveIfExists("SyncAllDqtInductionAuditsJob");
            recurringJobManager.RemoveIfExists("SyncAllDqtIttAuditsJob");
            recurringJobManager.RemoveIfExists("SyncAllDqtQtsAuditsJob");
            recurringJobManager.RemoveIfExists("SyncDqtContactAuditsMopUpJob");
            recurringJobManager.RemoveIfExists("ResyncAllPersonsJob");
            recurringJobManager.RemoveIfExists("CreatePersonMigratedEventsJob");
            recurringJobManager.RemoveIfExists("SyncAllDqtAnnotationAuditsJob");
            recurringJobManager.RemoveIfExists("BackfillPersonCreatedByTpsJob");
            recurringJobManager.RemoveIfExists("FixIncorrectOttRouteMigrationMappingsJob");
            recurringJobManager.RemoveIfExists("CpdInductionImporterJob");

            return Task.CompletedTask;
        });

        return services;
    }
}
