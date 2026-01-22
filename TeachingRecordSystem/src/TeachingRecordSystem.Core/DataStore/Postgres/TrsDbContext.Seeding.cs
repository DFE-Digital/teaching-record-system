using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

public partial class TrsDbContext
{
    /// <summary>
    /// Seeds the database with static reference data.
    /// This method is called by EF Core's UseSeeding configuration.
    /// </summary>
    private void SeedData()
    {
        SeedAlertCategories();
        SeedAlertTypes();
        SeedCountries();
        SeedInductionExemptionReasons();
        SeedSupportTaskTypes();
        SeedDegreeTypes();
        SeedEstablishmentSources();
        SeedInductionStatusInfo();
        SeedMandatoryQualificationProviders();
        SeedRouteToProfessionalStatusTypes();
        SeedTpsEstablishmentTypes();
        SeedTrainingProviders();
        SeedTrainingSubjects();
        SeedNameSynonyms();
        SeedPersonSearchAttributes();
        SeedPreviousNames();
        SeedApiKeys();
        SeedEstablishments();
        SeedEventTypes();
        SeedEmailJobs();
        SeedTpsCsvExtractItems();
        SeedTpsEmployments();
        SeedTpsEstablishments();
        SeedAlerts();
        SeedRouteMigrationReportItems();
        SeedUsers();
    }

    /// <summary>
    /// Seeds the database with static reference data asynchronously.
    /// This method is called by EF Core's UseAsyncSeeding configuration.
    /// </summary>
    private async Task SeedDataAsync(CancellationToken cancellationToken = default)
    {
        SeedAlertCategories();
        SeedAlertTypes();
        SeedCountries();
        SeedInductionExemptionReasons();
        SeedSupportTaskTypes();
        SeedDegreeTypes();
        SeedEstablishmentSources();
        SeedInductionStatusInfo();
        SeedMandatoryQualificationProviders();
        SeedRouteToProfessionalStatusTypes();
        SeedTpsEstablishmentTypes();
        SeedTrainingProviders();
        SeedTrainingSubjects();
        SeedNameSynonyms();
        SeedPersonSearchAttributes();
        SeedPreviousNames();
        SeedApiKeys();
        SeedEstablishments();
        SeedEventTypes();
        SeedEmailJobs();
        SeedTpsCsvExtractItems();
        SeedTpsEmployments();
        SeedTpsEstablishments();
        SeedAlerts();
        SeedRouteMigrationReportItems();
        SeedUsers();

        await Task.CompletedTask;
    }

    // Placeholder methods - will be implemented with actual seed data
    private void SeedAlertCategories()
    {
        // TODO: Implement
    }

    private void SeedAlertTypes()
    {
        // TODO: Implement
    }

    private void SeedCountries()
    {
        // TODO: Implement
    }

    private void SeedInductionExemptionReasons()
    {
        // TODO: Implement
    }

    private void SeedSupportTaskTypes()
    {
        // TODO: Implement
    }

    private void SeedDegreeTypes()
    {
        // TODO: Implement
    }

    private void SeedEstablishmentSources()
    {
        // TODO: Implement
    }

    private void SeedInductionStatusInfo()
    {
        // TODO: Implement
    }

    private void SeedMandatoryQualificationProviders()
    {
        // TODO: Implement
    }

    private void SeedRouteToProfessionalStatusTypes()
    {
        // TODO: Implement
    }

    private void SeedTpsEstablishmentTypes()
    {
        // TODO: Implement
    }

    private void SeedTrainingProviders()
    {
        // TODO: Implement
    }

    private void SeedTrainingSubjects()
    {
        // TODO: Implement
    }

    private void SeedNameSynonyms()
    {
        // TODO: Implement
    }

    private void SeedPersonSearchAttributes()
    {
        // TODO: Implement
    }

    private void SeedPreviousNames()
    {
        // TODO: Implement
    }

    private void SeedApiKeys()
    {
        // TODO: Implement
    }

    private void SeedEstablishments()
    {
        // TODO: Implement
    }

    private void SeedEventTypes()
    {
        // TODO: Implement
    }

    private void SeedEmailJobs()
    {
        // TODO: Implement
    }

    private void SeedTpsCsvExtractItems()
    {
        // TODO: Implement
    }

    private void SeedTpsEmployments()
    {
        // TODO: Implement
    }

    private void SeedTpsEstablishments()
    {
        // TODO: Implement
    }

    private void SeedAlerts()
    {
        // TODO: Implement
    }

    private void SeedRouteMigrationReportItems()
    {
        // TODO: Implement
    }

    private void SeedUsers()
    {
        // TODO: Implement
    }
}
