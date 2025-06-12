using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core;

public class ReferenceDataCache(
    ICrmQueryDispatcher crmQueryDispatcher,
    IDbContextFactory<TrsDbContext> dbContextFactory) : IStartupTask
{
    // CRM
    private Task<dfeta_mqestablishment[]>? _mqEstablishmentsTask;
    private Task<dfeta_sanctioncode[]>? _getSanctionCodesTask;
    private Task<Subject[]>? _getSubjectsTask;
    private Task<dfeta_teacherstatus[]>? _getTeacherStatusesTask;
    private Task<dfeta_earlyyearsstatus[]>? _getEarlyYearsStatusesTask;
    private Task<dfeta_specialism[]>? _getSpecialismsTask;
    private Task<dfeta_hequalification[]>? _getHeQualificationsTask;
    private Task<dfeta_hesubject[]>? _getHeSubjectsTask;
    private Task<dfeta_country[]>? _getCountriesTask;
    private Task<dfeta_ittsubject[]>? _getIttSubjectsTask;
    private Task<dfeta_ittqualification[]>? _getIttQualificationsTask;
    private Task<Account[]>? _getIttProvidersTask;

    // TRS
    private Task<AlertCategory[]>? _alertCategoriesTask;
    private Task<AlertType[]>? _alertTypesTask;
    private Task<InductionExemptionReason[]>? _inductionExemptionReasonsTask;
    private Task<RouteToProfessionalStatusType[]>? _routesTypesTask;
    private Task<TrainingSubject[]>? _trainingSubjectsTask;
    private Task<Country[]>? _countriesTask;
    private Task<TrainingProvider[]>? _trainingProvidersTask;
    private Task<DegreeType[]>? _degreeTypesTask;

    public async Task<dfeta_sanctioncode> GetSanctionCodeByValueAsync(string value)
    {
        var sanctionCodes = await EnsureSanctionCodesAsync();
        // build environment has some duplicate sanction codes, which prevent us using Single() here
        return sanctionCodes.First(s => s.dfeta_Value == value, $"Could not find sanction code with value: '{value}'.");
    }

    public async Task<dfeta_sanctioncode> GetSanctionCodeByIdAsync(Guid sanctionCodeId)
    {
        var sanctionCodes = await EnsureSanctionCodesAsync();
        return sanctionCodes.Single(s => s.dfeta_sanctioncodeId == sanctionCodeId, $"Could not find sanction code with ID: '{sanctionCodeId}'.");
    }

    public async Task<dfeta_sanctioncode[]> GetSanctionCodesAsync(bool activeOnly = true)
    {
        var sanctionCodes = await EnsureSanctionCodesAsync();
        return sanctionCodes.Where(s => s.StateCode == dfeta_sanctioncodeState.Active || !activeOnly).ToArray();
    }

    public async Task<Subject> GetSubjectByTitleAsync(string title)
    {
        var subjects = await EnsureSubjectsAsync();
        return subjects.Single(s => s.Title == title, $"Could not find subject with title: '{title}'.");
    }

    public async Task<dfeta_teacherstatus> GetTeacherStatusByIdAsync(Guid id)
    {
        var teacherStatuses = await EnsureTeacherStatusesAsync();
        return teacherStatuses.Single(ts => ts.Id == id, $"Could not find teacher status with ID: '{id}'.");
    }

    public async Task<dfeta_teacherstatus> GetTeacherStatusByValueAsync(string value)
    {
        var teacherStatuses = await EnsureTeacherStatusesAsync();
        return teacherStatuses.Single(ts => ts.dfeta_Value == value, $"Could not find teacher status with value: '{value}'.");
    }

    public async Task<dfeta_teacherstatus[]> GetTeacherStatusesAsync()
    {
        var teacherStatuses = await EnsureTeacherStatusesAsync();
        return teacherStatuses;
    }

    public async Task<dfeta_earlyyearsstatus[]> GetEytsStatusesAsync()
    {
        var earlyyearStatuses = await EnsureEarlyYearsStatusesAsync();
        return earlyyearStatuses;
    }

    public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatusByIdAsync(Guid id)
    {
        var earlyYearsStatuses = await EnsureEarlyYearsStatusesAsync();
        return earlyYearsStatuses.Single(ey => ey.Id == id, $"Could not find early years teacher status with ID: '{id}'.");
    }

    public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatusByValueAsync(string value)
    {
        var earlyYearsStatuses = await EnsureEarlyYearsStatusesAsync();
        return earlyYearsStatuses.Single(ey => ey.dfeta_Value == value, $"Could not find early years teacher status with value: '{value}'.");
    }

    public async Task<dfeta_specialism[]> GetMqSpecialismsAsync()
    {
        var specialisms = await EnsureSpecialismsAsync();
        return specialisms.ToArray();
    }

    public async Task<dfeta_specialism> GetMqSpecialismByValueAsync(string value)
    {
        var specialisms = await EnsureSpecialismsAsync();
        // build environment has some duplicate Specialisms, which prevent us using Single() here
        return specialisms.First(s => s.dfeta_Value == value, $"Could not find MQ specialism with value: '{value}'.");
    }

    public async Task<dfeta_specialism> GetMqSpecialismByIdAsync(Guid specialismId)
    {
        var specialisms = await EnsureSpecialismsAsync();
        return specialisms.Single(s => s.dfeta_specialismId == specialismId, $"Could not find MQ specialism with ID: '{specialismId}'.");
    }

    public async Task<dfeta_mqestablishment[]> GetMqEstablishmentsAsync()
    {
        var mqEstablishments = await EnsureMqEstablishmentsAsync();
        return mqEstablishments.ToArray();
    }

    public async Task<dfeta_mqestablishment> GetMqEstablishmentByValueAsync(string value)
    {
        var mqEstablishments = await EnsureMqEstablishmentsAsync();
        // build environment has some duplicate MQ Establishments, which prevent us using Single() here
        return mqEstablishments.First(s => s.dfeta_Value == value, $"Could not find MQ establishment with value: '{value}'.");
    }

    public async Task<dfeta_mqestablishment> GetMqEstablishmentByIdAsync(Guid mqEstablishmentId)
    {
        var mqEstablishments = await EnsureMqEstablishmentsAsync();
        return mqEstablishments.Single(s => s.dfeta_mqestablishmentId == mqEstablishmentId, $"Could not find MQ establishment with ID: '{mqEstablishmentId}'.");
    }

    public async Task<dfeta_hequalification[]> GetHeQualificationsAsync()
    {
        var heQualifications = await EnsureHeQualificationsAsync();
        return heQualifications.ToArray();
    }

    public async Task<dfeta_hequalification> GetHeQualificationByValueAsync(string value)
    {
        var heQualifications = await EnsureHeQualificationsAsync();
        // build environment has some duplicate HE Qualifications, which prevent us using Single() here
        return heQualifications.First(s => s.dfeta_Value == value, $"Could not find HE qualification with value: '{value}'.");
    }

    public async Task<dfeta_hesubject[]> GetHeSubjectsAsync()
    {
        var heSubjects = await EnsureHeSubjectsAsync();
        return heSubjects.ToArray();
    }

    public async Task<dfeta_hesubject> GetHeSubjectByValueAsync(string value)
    {
        var heSubjects = await EnsureHeSubjectsAsync();
        // build environment has some duplicate HE Subjects, which prevent us using Single() here
        return heSubjects.First(s => s.dfeta_Value == value, $"Could not find HE subject with value: '{value}'.");
    }

    public async Task<AlertCategory[]> GetAlertCategoriesAsync()
    {
        var alertCategories = await EnsureAlertCategoriesAsync();
        return alertCategories;
    }

    public async Task<AlertCategory> GetAlertCategoryByIdAsync(Guid alertCategoryId)
    {
        var alertCategories = await EnsureAlertCategoriesAsync();
        return alertCategories.Single(ac => ac.AlertCategoryId == alertCategoryId, $"Could not find alert category with ID: '{alertCategoryId}'.");
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

    public async Task<AlertType?> GetAlertTypeByDqtSanctionCodeIfExistsAsync(string dqtSanctionCode)
    {
        var alertTypes = await EnsureAlertTypesAsync();
        return alertTypes.SingleOrDefault(at => at.DqtSanctionCode == dqtSanctionCode);
    }

    public async Task<dfeta_country?> GetCountryByCountryCodeAsync(string countryCode)
    {
        var countries = await EnsureCountriesAsync();
        // build environment has duplicate Countries, which prevent us using Single() here
        return countries.FirstOrDefault(at => at.dfeta_Value == countryCode);
    }

    public async Task<dfeta_ittsubject?> GetIttSubjectBySubjectCodeAsync(string subjectCode)
    {
        var ittSubjects = await EnsureIttSubjectsAsync();
        // build environment has duplicate ITT Subjects, which prevent us using Single() here
        return ittSubjects.FirstOrDefault(at => at.dfeta_Value == subjectCode);
    }

    public async Task<dfeta_ittqualification[]> GetIttQualificationsAsync()
    {
        var ittQualifications = await EnsureIttQualificationsAsync();
        return ittQualifications.ToArray();
    }

    public async Task<dfeta_ittqualification> GetIttQualificationByValueAsync(string value)
    {
        var ittQualifications = await EnsureIttQualificationsAsync();
        // build environment has some duplicate ITT Qualifications, which prevent us using Single() here
        return ittQualifications.First(s => s.dfeta_Value == value, $"Could not find ITT qualification with value: '{value}'.");
    }

    public async Task<Account?> GetIttProviderByUkPrnAsync(string ukPrn)
    {
        var ittProviders = await EnsureIttProvidersAsync();
        return ittProviders.SingleOrDefault(p => p.dfeta_UKPRN == ukPrn);
    }

    public async Task<Account?> GetIttProviderByNameAsync(string name)
    {
        var ittProviders = await EnsureIttProvidersAsync();
        return ittProviders.SingleOrDefault(p => p.Name == name);
    }

    public async Task<InductionExemptionReason[]> GetPersonLevelInductionExemptionReasonsAsync(bool activeOnly = false)
    {
        var inductionExemptionReasons = await EnsureInductionExemptionReasonsAsync();
        return inductionExemptionReasons.Where(e => !e.RouteOnlyExemption && !activeOnly || e.IsActive).ToArray();
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

    public async Task<TrainingSubject> GetTrainingSubjectsByIdAsync(Guid id)
    {
        var trainingSubjects = await EnsureTrainingSubjectsAsync();
        return trainingSubjects.Single(e => e.TrainingSubjectId == id, $"Could not find subject with ID: '{id}'.");
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

    private Task<dfeta_sanctioncode[]> EnsureSanctionCodesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getSanctionCodesTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllSanctionCodesQuery(ActiveOnly: false)));

    private Task<Subject[]> EnsureSubjectsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getSubjectsTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllSubjectsQuery()));

    private Task<dfeta_teacherstatus[]> EnsureTeacherStatusesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getTeacherStatusesTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllTeacherStatusesQuery()));

    private Task<dfeta_earlyyearsstatus[]> EnsureEarlyYearsStatusesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getEarlyYearsStatusesTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllActiveEarlyYearsStatusesQuery()));

    private Task<dfeta_specialism[]> EnsureSpecialismsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getSpecialismsTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllSpecialismsQuery()));

    private Task<dfeta_mqestablishment[]> EnsureMqEstablishmentsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _mqEstablishmentsTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllMqEstablishmentsQuery()));

    private Task<dfeta_hequalification[]> EnsureHeQualificationsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getHeQualificationsTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllActiveHeQualificationsQuery()));

    private Task<dfeta_hesubject[]> EnsureHeSubjectsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getHeSubjectsTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllActiveHeSubjectsQuery()));

    private Task<AlertCategory[]> EnsureAlertCategoriesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _alertCategoriesTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.AlertCategories.AsNoTracking().Include(c => c.AlertTypes).IgnoreAutoIncludes().ToArrayAsync();
            });

    private Task<AlertType[]> EnsureAlertTypesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _alertTypesTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.AlertTypes.AsNoTracking().Include(t => t.AlertCategory).IgnoreAutoIncludes().ToArrayAsync();
            });

    private Task<dfeta_country[]> EnsureCountriesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getCountriesTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllCountriesQuery()));

    private Task<dfeta_ittsubject[]> EnsureIttSubjectsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getIttSubjectsTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllActiveIttSubjectsQuery()));

    private Task<dfeta_ittqualification[]> EnsureIttQualificationsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getIttQualificationsTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllActiveIttQualificationsQuery()));

    private Task<Account[]> EnsureIttProvidersAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _getIttProvidersTask,
            () => crmQueryDispatcher.ExecuteQueryAsync(new GetAllIttProvidersQuery()));

    private Task<InductionExemptionReason[]> EnsureInductionExemptionReasonsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _inductionExemptionReasonsTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.InductionExemptionReasons.AsNoTracking().ToArrayAsync();
            });

    private Task<RouteToProfessionalStatusType[]> EnsureRouteToProfessionalStatusTypesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _routesTypesTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.RoutesToProfessionalStatus.AsNoTracking().ToArrayAsync();
            });

    private Task<Country[]> EnsureTrainingCountriesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _countriesTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.Countries.AsNoTracking().ToArrayAsync();
            });

    private Task<TrainingSubject[]> EnsureTrainingSubjectsAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _trainingSubjectsTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.TrainingSubjects.AsNoTracking().ToArrayAsync();
            });

    private Task<TrainingProvider[]> EnsureTrainingProvidersAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _trainingProvidersTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.TrainingProviders.AsNoTracking().ToArrayAsync();
            });

    private Task<DegreeType[]> EnsureDegreeTypesAsync() =>
        LazyInitializer.EnsureInitialized(
            ref _degreeTypesTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.DegreeTypes.AsNoTracking().ToArrayAsync();
            });

    async Task IStartupTask.ExecuteAsync()
    {
        // CRM
        await EnsureSanctionCodesAsync();
        await EnsureSubjectsAsync();
        await EnsureTeacherStatusesAsync();
        await EnsureEarlyYearsStatusesAsync();
        await EnsureSpecialismsAsync();
        await EnsureMqEstablishmentsAsync();
        await EnsureHeQualificationsAsync();
        await EnsureHeSubjectsAsync();
        await EnsureCountriesAsync();
        await EnsureIttSubjectsAsync();
        await EnsureIttQualificationsAsync();
        await EnsureIttProvidersAsync();

        // TRS
        await EnsureAlertCategoriesAsync();
        await EnsureAlertTypesAsync();
        await EnsureInductionExemptionReasonsAsync();
        await EnsureRouteToProfessionalStatusTypesAsync();
        await EnsureTrainingCountriesAsync();
        await EnsureTrainingSubjectsAsync();
        await EnsureTrainingProvidersAsync();
    }
}
