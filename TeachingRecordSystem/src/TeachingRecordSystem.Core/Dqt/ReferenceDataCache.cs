using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt;

public class ReferenceDataCache
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private Task<dfeta_sanctioncode[]>? _getSanctionCodesTask;
    private Task<Subject[]>? _getSubjectsTask;
    private Task<dfeta_teacherstatus[]>? _getTeacherStatusesTask;
    private Task<dfeta_earlyyearsstatus[]>? _getEarlyYearsStatusesTask;

    public ReferenceDataCache(ICrmQueryDispatcher crmQueryDispatcher)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    public async Task<dfeta_sanctioncode> GetSanctionCodeByValue(string value)
    {
        var sanctionCodes = await EnsureSanctionCodes();
        // build environment has some duplicate sanction codes, which prevent us using Single() here
        return sanctionCodes.First(s => s.dfeta_Value == value);
    }

    public async Task<dfeta_sanctioncode> GetSanctionCodeById(Guid sanctionCodeId)
    {
        var sanctionCodes = await EnsureSanctionCodes();
        return sanctionCodes.Single(s => s.dfeta_sanctioncodeId == sanctionCodeId);
    }

    public async Task<dfeta_sanctioncode[]> GetSanctionCodes()
    {
        var sanctionCodes = await EnsureSanctionCodes();
        return sanctionCodes.ToArray();
    }

    public async Task<Subject> GetSubjectByTitle(string title)
    {
        var subjects = await EnsureSubjects();
        return subjects.Single(s => s.Title == title);
    }

    public async Task<dfeta_teacherstatus> GetTeacherStatusByValue(string value)
    {
        var teacherStatuses = await EnsureTeacherStatuses();
        return teacherStatuses.Single(ts => ts.dfeta_Value == value);
    }

    public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatusByValue(string value)
    {
        var earlyYearsStatuses = await EnsureEarlyYearsStatuses();
        return earlyYearsStatuses.Single(ey => ey.dfeta_Value == value);
    }

    private Task<dfeta_sanctioncode[]> EnsureSanctionCodes() =>
        LazyInitializer.EnsureInitialized(
            ref _getSanctionCodesTask,
            () => _crmQueryDispatcher.ExecuteQuery(new GetAllSanctionCodesQuery()));

    private Task<Subject[]> EnsureSubjects() =>
        LazyInitializer.EnsureInitialized(
            ref _getSubjectsTask,
            () => _crmQueryDispatcher.ExecuteQuery(new GetAllSubjectsQuery()));

    private Task<dfeta_teacherstatus[]> EnsureTeacherStatuses() =>
        LazyInitializer.EnsureInitialized(
            ref _getTeacherStatusesTask,
            () => _crmQueryDispatcher.ExecuteQuery(new GetAllTeacherStatusesQuery()));

    // ensure early years statuses
    private Task<dfeta_earlyyearsstatus[]> EnsureEarlyYearsStatuses() =>
        LazyInitializer.EnsureInitialized(
            ref _getEarlyYearsStatusesTask,
            () => _crmQueryDispatcher.ExecuteQuery(new GetAllEarlyYearsStatusesQuery()));
}
