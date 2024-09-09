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

    // TRS
    private Task<AlertCategory[]>? _alertCategoriesTask;
    private Task<AlertType[]>? _alertTypesTask;

    public async Task<dfeta_sanctioncode> GetSanctionCodeByValue(string value)
    {
        var sanctionCodes = await EnsureSanctionCodes();
        // build environment has some duplicate sanction codes, which prevent us using Single() here
        return sanctionCodes.First(s => s.dfeta_Value == value, $"Could not find sanction code with value: '{value}'.");
    }

    public async Task<dfeta_sanctioncode> GetSanctionCodeById(Guid sanctionCodeId)
    {
        var sanctionCodes = await EnsureSanctionCodes();
        return sanctionCodes.Single(s => s.dfeta_sanctioncodeId == sanctionCodeId, $"Could not find sanction code with ID: '{sanctionCodeId}'.");
    }

    public async Task<dfeta_sanctioncode[]> GetSanctionCodes()
    {
        var sanctionCodes = await EnsureSanctionCodes();
        return sanctionCodes.ToArray();
    }

    public async Task<Subject> GetSubjectByTitle(string title)
    {
        var subjects = await EnsureSubjects();
        return subjects.Single(s => s.Title == title, $"Could not find subject with title: '{title}'.");
    }

    public async Task<dfeta_teacherstatus> GetTeacherStatusById(Guid id)
    {
        var teacherStatuses = await EnsureTeacherStatuses();
        return teacherStatuses.Single(ts => ts.Id == id, $"Could not find teacher status with ID: '{id}'.");
    }

    public async Task<dfeta_teacherstatus> GetTeacherStatusByValue(string value)
    {
        var teacherStatuses = await EnsureTeacherStatuses();
        return teacherStatuses.Single(ts => ts.dfeta_Value == value, $"Could not find teacher status with value: '{value}'.");
    }

    public async Task<dfeta_teacherstatus[]> GetTeacherStatuses()
    {
        var teacherStatuses = await EnsureTeacherStatuses();
        return teacherStatuses;
    }

    public async Task<dfeta_earlyyearsstatus[]> GetEytsStatuses()
    {
        var earlyyearStatuses = await EnsureEarlyYearsStatuses();
        return earlyyearStatuses;
    }

    public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatusById(Guid id)
    {
        var earlyYearsStatuses = await EnsureEarlyYearsStatuses();
        return earlyYearsStatuses.Single(ey => ey.Id == id, $"Could not find early years teacher status with ID: '{id}'.");
    }

    public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatusByValue(string value)
    {
        var earlyYearsStatuses = await EnsureEarlyYearsStatuses();
        return earlyYearsStatuses.Single(ey => ey.dfeta_Value == value, $"Could not find early years teacher status with value: '{value}'.");
    }

    public async Task<dfeta_specialism[]> GetMqSpecialisms()
    {
        var specialisms = await EnsureSpecialisms();
        return specialisms.ToArray();
    }

    public async Task<dfeta_specialism> GetMqSpecialismByValue(string value)
    {
        var specialisms = await EnsureSpecialisms();
        // build environment has some duplicate Specialisms, which prevent us using Single() here
        return specialisms.First(s => s.dfeta_Value == value, $"Could not find MQ specialism with value: '{value}'.");
    }

    public async Task<dfeta_specialism> GetMqSpecialismById(Guid specialismId)
    {
        var specialisms = await EnsureSpecialisms();
        return specialisms.Single(s => s.dfeta_specialismId == specialismId, $"Could not find MQ specialism with ID: '{specialismId}'.");
    }

    public async Task<dfeta_mqestablishment[]> GetMqEstablishments()
    {
        var mqEstablishments = await EnsureMqEstablishments();
        return mqEstablishments.ToArray();
    }

    public async Task<dfeta_mqestablishment> GetMqEstablishmentByValue(string value)
    {
        var mqEstablishments = await EnsureMqEstablishments();
        // build environment has some duplicate MQ Establishments, which prevent us using Single() here
        return mqEstablishments.First(s => s.dfeta_Value == value, $"Could not find MQ establishment with value: '{value}'.");
    }

    public async Task<dfeta_mqestablishment> GetMqEstablishmentById(Guid mqEstablishmentId)
    {
        var mqEstablishments = await EnsureMqEstablishments();
        return mqEstablishments.Single(s => s.dfeta_mqestablishmentId == mqEstablishmentId, $"Could not find MQ establishment with ID: '{mqEstablishmentId}'.");
    }

    public async Task<dfeta_hequalification[]> GetHeQualifications()
    {
        var heQualifications = await EnsureHeQualifications();
        return heQualifications.ToArray();
    }

    public async Task<dfeta_hequalification> GetHeQualificationByValue(string value)
    {
        var heQualifications = await EnsureHeQualifications();
        // build environment has some duplicate HE Qualifications, which prevent us using Single() here
        return heQualifications.First(s => s.dfeta_Value == value, $"Could not find HE qualification with value: '{value}'.");
    }

    public async Task<dfeta_hesubject[]> GetHeSubjects()
    {
        var heSubjects = await EnsureHeSubjects();
        return heSubjects.ToArray();
    }

    public async Task<dfeta_hesubject> GetHeSubjectByValue(string value)
    {
        var heSubjects = await EnsureHeSubjects();
        // build environment has some duplicate HE Subjects, which prevent us using Single() here
        return heSubjects.First(s => s.dfeta_Value == value, $"Could not find HE subject with value: '{value}'.");
    }

    public async Task<AlertCategory[]> GetAlertCategories()
    {
        var alertCategories = await EnsureAlertCategories();
        return alertCategories;
    }

    public async Task<AlertCategory> GetAlertCategoryById(Guid alertCategoryId)
    {
        var alertCategories = await EnsureAlertCategories();
        return alertCategories.Single(ac => ac.AlertCategoryId == alertCategoryId, $"Could not find alert category with ID: '{alertCategoryId}'.");
    }

    public async Task<AlertType[]> GetAlertTypes(bool activeOnly = false)
    {
        var alertTypes = await EnsureAlertTypes();
        return alertTypes.Where(t => !activeOnly || t.IsActive).ToArray();
    }

    public async Task<AlertType> GetAlertTypeById(Guid alertTypeId)
    {
        var alertTypes = await EnsureAlertTypes();
        return alertTypes.Single(at => at.AlertTypeId == alertTypeId, $"Could not find alert type with ID: '{alertTypeId}'.");
    }

    public async Task<AlertType> GetAlertTypeByDqtSanctionCode(string dqtSanctionCode)
    {
        var alertTypes = await EnsureAlertTypes();
        return alertTypes.Single(at => at.DqtSanctionCode == dqtSanctionCode, $"Could not find alert type with DQT sanction code: '{dqtSanctionCode}'.");
    }

    public async Task<AlertType?> GetAlertTypeByDqtSanctionCodeIfExists(string dqtSanctionCode)
    {
        var alertTypes = await EnsureAlertTypes();
        return alertTypes.SingleOrDefault(at => at.DqtSanctionCode == dqtSanctionCode);
    }

    private Task<dfeta_sanctioncode[]> EnsureSanctionCodes() =>
        LazyInitializer.EnsureInitialized(
            ref _getSanctionCodesTask,
            () => crmQueryDispatcher.ExecuteQuery(new GetAllActiveSanctionCodesQuery()));

    private Task<Subject[]> EnsureSubjects() =>
        LazyInitializer.EnsureInitialized(
            ref _getSubjectsTask,
            () => crmQueryDispatcher.ExecuteQuery(new GetAllSubjectsQuery()));

    private Task<dfeta_teacherstatus[]> EnsureTeacherStatuses() =>
        LazyInitializer.EnsureInitialized(
            ref _getTeacherStatusesTask,
            () => crmQueryDispatcher.ExecuteQuery(new GetAllTeacherStatusesQuery()));

    private Task<dfeta_earlyyearsstatus[]> EnsureEarlyYearsStatuses() =>
        LazyInitializer.EnsureInitialized(
            ref _getEarlyYearsStatusesTask,
            () => crmQueryDispatcher.ExecuteQuery(new GetAllActiveEarlyYearsStatusesQuery()));

    private Task<dfeta_specialism[]> EnsureSpecialisms() =>
        LazyInitializer.EnsureInitialized(
            ref _getSpecialismsTask,
            () => crmQueryDispatcher.ExecuteQuery(new GetAllSpecialismsQuery()));

    private Task<dfeta_mqestablishment[]> EnsureMqEstablishments() =>
        LazyInitializer.EnsureInitialized(
            ref _mqEstablishmentsTask,
            () => crmQueryDispatcher.ExecuteQuery(new GetAllMqEstablishmentsQuery()));

    private Task<dfeta_hequalification[]> EnsureHeQualifications() =>
        LazyInitializer.EnsureInitialized(
            ref _getHeQualificationsTask,
            () => crmQueryDispatcher.ExecuteQuery(new GetAllActiveHeQualificationsQuery()));

    private Task<dfeta_hesubject[]> EnsureHeSubjects() =>
        LazyInitializer.EnsureInitialized(
            ref _getHeSubjectsTask,
            () => crmQueryDispatcher.ExecuteQuery(new GetAllActiveHeSubjectsQuery()));

    private Task<AlertCategory[]> EnsureAlertCategories() =>
        LazyInitializer.EnsureInitialized(
            ref _alertCategoriesTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.AlertCategories.Include(c => c.AlertTypes).ToArrayAsync();
            });

    private Task<AlertType[]> EnsureAlertTypes() =>
        LazyInitializer.EnsureInitialized(
            ref _alertTypesTask,
            async () =>
            {
                using var dbContext = dbContextFactory.CreateDbContext();
                return await dbContext.AlertTypes.ToArrayAsync();
            });

    async Task IStartupTask.Execute()
    {
        // CRM
        await EnsureSanctionCodes();
        await EnsureSubjects();
        await EnsureTeacherStatuses();
        await EnsureEarlyYearsStatuses();
        await EnsureSpecialisms();
        await EnsureMqEstablishments();
        await EnsureHeQualifications();
        await EnsureHeSubjects();

        // TRS
        await EnsureAlertCategories();
        await EnsureAlertTypes();
    }
}
