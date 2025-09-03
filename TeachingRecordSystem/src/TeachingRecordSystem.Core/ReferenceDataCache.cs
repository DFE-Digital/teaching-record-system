using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core;

public class ReferenceDataCache(IDbContextFactory<TrsDbContext> dbContextFactory) : IStartupTask
{
    private object _alertCategoriesSyncObj = new();
    private object _alertTypesSyncObj = new();
    private object _inductionExemptionReasonsSyncObj = new();
    private object _routeTypesSyncObj = new();
    private object _trainingSubjectsSyncObj = new();
    private object _countriesSyncObj = new();
    private object _trainingProvidersSyncObj = new();
    private object _degreeTypesSyncObj = new();

    private Task<AlertCategory[]>? _alertCategoriesTask;
    private Task<AlertType[]>? _alertTypesTask;
    private Task<InductionExemptionReason[]>? _inductionExemptionReasonsTask;
    private Task<RouteToProfessionalStatusType[]>? _routesTypesTask;
    private Task<TrainingSubject[]>? _trainingSubjectsTask;
    private Task<Country[]>? _countriesTask;
    private Task<TrainingProvider[]>? _trainingProvidersTask;
    private Task<DegreeType[]>? _degreeTypesTask;

    public void Clear()
    {
        _alertCategoriesTask = null;
        _alertTypesTask = null;
        _inductionExemptionReasonsTask = null;
        _routesTypesTask = null;
        _trainingSubjectsTask = null;
        _countriesTask = null;
        _trainingProvidersTask = null;
        _degreeTypesTask = null;
    }

    public async Task<AlertCategory[]> GetAlertCategoriesAsync()
    {
        var alertCategories = await EnsureAlertCategoriesAsync();
        return alertCategories;
    }

    public async Task<AlertType[]> GetAlertTypesAsync(bool activeOnly = false)
    {
        var alertTypes = await EnsureAlertTypesAsync();
        return alertTypes.Where(t => !activeOnly || t.IsActive).ToArray();
    }

    public async Task<AlertType> GetAlertTypeByIdAsync(Guid alertTypeId)
    {
        var alertTypes = await EnsureAlertTypesAsync();
        return alertTypes.Single(at => at.AlertTypeId == alertTypeId, $"Could not find alert type with ID: '{alertTypeId}'.");
    }

    public async Task<AlertType> GetAlertTypeByDqtSanctionCodeAsync(string dqtSanctionCode)
    {
        var alertTypes = await EnsureAlertTypesAsync();
        return alertTypes.Single(at => at.DqtSanctionCode == dqtSanctionCode, $"Could not find alert type with DQT sanction code: '{dqtSanctionCode}'.");
    }

    public async Task<InductionExemptionReason[]> GetPersonLevelInductionExemptionReasonsAsync(bool activeOnly = false)
    {
        var inductionExemptionReasons = await EnsureInductionExemptionReasonsAsync();
        return inductionExemptionReasons.Where(e => !e.RouteOnlyExemption && (!activeOnly || e.IsActive)).ToArray();
    }

    public async Task<InductionExemptionReason[]> GetInductionExemptionReasonsAsync(bool activeOnly = false)
    {
        var inductionExemptionReasons = await EnsureInductionExemptionReasonsAsync();
        return inductionExemptionReasons.Where(e => !activeOnly || e.IsActive).ToArray();
    }

    public async Task<InductionExemptionReason> GetInductionExemptionReasonByIdAsync(Guid id)
    {
        var inductionExemptionReasons = await EnsureInductionExemptionReasonsAsync();
        return inductionExemptionReasons.Single(er => er.InductionExemptionReasonId == id, $"Could not find induction exemption reason with ID: '{id}'.");
    }

    public async Task<RouteToProfessionalStatusType[]> GetRouteToProfessionalStatusTypesAsync(bool activeOnly = false)
    {
        var routeTypes = await EnsureRouteToProfessionalStatusTypesAsync();
        return routeTypes.Where(e => !activeOnly || e.IsActive).OrderBy(x => x.Name).ToArray();
    }

    public async Task<RouteToProfessionalStatusType> GetRouteToProfessionalStatusTypeByIdAsync(Guid id)
    {
        var routeTypes = await EnsureRouteToProfessionalStatusTypesAsync();
        return routeTypes.Single(r => r.RouteToProfessionalStatusTypeId == id, $"Could not find route to professional status with ID: '{id}'.");
    }

    public async Task<TrainingSubject[]> GetTrainingSubjectsAsync(bool activeOnly = false)
    {
        var trainingSubjects = await EnsureTrainingSubjectsAsync();
        return trainingSubjects.Where(e => !activeOnly || e.IsActive).OrderBy(s => s.Name).ToArray();
    }

    public async Task<TrainingSubject> GetTrainingSubjectByIdAsync(Guid id)
    {
        var trainingSubjects = await EnsureTrainingSubjectsAsync();
        return trainingSubjects.Single(e => e.TrainingSubjectId == id, $"Could not find subject with ID: '{id}'.");
    }

    public async Task<TrainingSubject?> GetTrainingSubjectByReferenceAsync(string reference)
    {
        var trainingSubjects = await EnsureTrainingSubjectsAsync();
        return trainingSubjects.Single(e => e.Reference == reference, $"Could not find subject with reference: '{reference}'.");
    }

    public async Task<Country[]> GetTrainingCountriesAsync()
    {
        return (await EnsureTrainingCountriesAsync()).OrderBy(x => x.Name).ToArray();
    }

    public async Task<Country> GetTrainingCountryByIdAsync(string countryId)
    {
        var countries = await EnsureTrainingCountriesAsync();
        return countries.Single(c => c.CountryId == countryId, $"Could not find country with ID: '{countryId}'.");
    }

    public async Task<DegreeType[]> GetDegreeTypesAsync()
    {
        return (await EnsureDegreeTypesAsync()).OrderBy(x => x.Name).ToArray();
    }

    public async Task<DegreeType> GetDegreeTypeByIdAsync(Guid degreeTypeId)
    {
        var degreeTypes = await EnsureDegreeTypesAsync();
        return degreeTypes.Single(dt => dt.DegreeTypeId == degreeTypeId, $"Could not find degree type with ID: '{degreeTypeId}'.");
    }

    public async Task<TrainingProvider[]> GetTrainingProvidersAsync(bool activeOnly = false)
    {
        var trainingProviders = await EnsureTrainingProvidersAsync();
        return trainingProviders.Where(e => !activeOnly || e.IsActive).ToArray();
    }

    public async Task<TrainingProvider> GetTrainingProviderByIdAsync(Guid trainingProviderId)
    {
        var trainingProviders = await EnsureTrainingProvidersAsync();
        return trainingProviders.Single(tp => tp.TrainingProviderId == trainingProviderId, $"Could not find training provider with ID: '{trainingProviderId}'.");
    }

    public async Task<TrainingProvider?> GetTrainingProviderByUkprnAsync(string ukprn)
    {
        var trainingProviders = await EnsureTrainingProvidersAsync();
        return trainingProviders.SingleOrDefault(tp => tp.Ukprn == ukprn);
    }

    private Task<AlertCategory[]> EnsureAlertCategoriesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _alertCategoriesTask,
            ref _alertCategoriesSyncObj,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.AlertCategories.AsNoTracking().Include(c => c.AlertTypes).IgnoreAutoIncludes().ToArrayAsync();
            });

    private Task<AlertType[]> EnsureAlertTypesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _alertTypesTask,
            ref _alertTypesSyncObj,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.AlertTypes.AsNoTracking().Include(t => t.AlertCategory).IgnoreAutoIncludes().ToArrayAsync();
            });

    private Task<InductionExemptionReason[]> EnsureInductionExemptionReasonsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _inductionExemptionReasonsTask,
            ref _inductionExemptionReasonsSyncObj,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.InductionExemptionReasons.AsNoTracking().ToArrayAsync();
            });

    private Task<RouteToProfessionalStatusType[]> EnsureRouteToProfessionalStatusTypesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _routesTypesTask,
            ref _routeTypesSyncObj,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.RouteToProfessionalStatusTypes.AsNoTracking().ToArrayAsync();
            });

    private Task<Country[]> EnsureTrainingCountriesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _countriesTask,
            ref _countriesSyncObj,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.Countries.AsNoTracking().ToArrayAsync();
            });

    private Task<TrainingSubject[]> EnsureTrainingSubjectsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _trainingSubjectsTask,
            ref _trainingSubjectsSyncObj,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.TrainingSubjects.AsNoTracking().ToArrayAsync();
            });

    private Task<TrainingProvider[]> EnsureTrainingProvidersAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _trainingProvidersTask,
            ref _trainingProvidersSyncObj,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.TrainingProviders.AsNoTracking().ToArrayAsync();
            });

    private Task<DegreeType[]> EnsureDegreeTypesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _degreeTypesTask,
            ref _degreeTypesSyncObj,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.DegreeTypes.AsNoTracking().ToArrayAsync();
            });

    Task IStartupTask.ExecuteAsync() => Task.WhenAll(
        EnsureAlertCategoriesAsync(),
        EnsureAlertTypesAsync(),
        EnsureInductionExemptionReasonsAsync(),
        EnsureRouteToProfessionalStatusTypesAsync(),
        EnsureTrainingSubjectsAsync(),
        EnsureTrainingCountriesAsync(),
        EnsureTrainingProvidersAsync(),
        EnsureDegreeTypesAsync()
    );
}
