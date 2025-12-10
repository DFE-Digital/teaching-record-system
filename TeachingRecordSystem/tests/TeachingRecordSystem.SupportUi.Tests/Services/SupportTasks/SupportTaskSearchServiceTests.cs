using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public partial class SupportTaskSearchServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
}

public class TaskLookup : Dictionary<string, SupportTask>
{
    private Lazy<Dictionary<string, string>> _keys;

    public TaskLookup()
    {
        _keys = new(() => this.ToDictionary(t => t.Value.SupportTaskReference, t => t.Key));
    }

    public TaskLookup(IDictionary<string, SupportTask> tasks) : base(tasks)
    {
        _keys = new(() => this.ToDictionary(t => t.Value.SupportTaskReference, t => t.Key));
    }

    public string GetKeyFor(string supportTaskRef) => _keys.Value[supportTaskRef];

    public IEnumerable<string> GetKeysFor<T>(ResultPage<T> searchResults) where T : ISupportTaskSearchResult
        => searchResults.Select(r => GetKeyFor(r.SupportTaskReference));

    public static TaskLookup Create(Dictionary<string, ISupportTaskCreateResult> results)
         => new(results.ToDictionary(r => r.Key, r => r.Value.SupportTask));

}

//public class TaskLookup
//{
//    private Dictionary<string, SupportTask> _tasks;
//    private Lazy<Dictionary<string, string>> _keys;

//    public TaskLookup(Dictionary<string, SupportTask> tasks)
//    {
//        _tasks = tasks;
//        _keys = new(() => tasks.ToDictionary(t => t.Value.SupportTaskReference, t => t.Key));
//    }

//    public SupportTask this[string key] => _tasks[key];

//    public string GetKeyFor(string supportTaskRef) => _keys.Value[supportTaskRef];

//    public IEnumerable<string> GetKeysFor<T>(ResultPage<T> searchResults) where T : ISupportTaskSearchResult
//        => searchResults.Select(r => GetKeyFor(r.SupportTaskReference));

//    public static TaskLookup Create(Dictionary<string, ISupportTaskCreateResult> results)
//        => new(results.ToDictionary(r => r.Key, r => r.Value.SupportTask));

//    public static TaskLookup Create(Dictionary<string, SupportTask> tasks)
//        => new(tasks);
//}
