using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt;

public class ReferenceDataCache
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private Task<Subject[]>? _subjects;
    private Task<dfeta_teacherstatus[]>? _getTeacherStatusesTask;

    public ReferenceDataCache(ICrmQueryDispatcher crmQueryDispatcher)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
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

    private Task<Subject[]> EnsureSubjects() =>
        LazyInitializer.EnsureInitialized(
            ref _subjects,
            () => _crmQueryDispatcher.ExecuteQuery(new GetAllSubjectsQuery()));

    private Task<dfeta_teacherstatus[]> EnsureTeacherStatuses() =>
        LazyInitializer.EnsureInitialized(
            ref _getTeacherStatusesTask,
            () => _crmQueryDispatcher.ExecuteQuery(new GetAllTeacherStatusesQuery()));
}
