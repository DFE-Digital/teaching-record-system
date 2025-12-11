using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

public class SupportTaskLookup : Dictionary<string, SupportTask>
{
    private Lazy<Dictionary<string, string>> _keys;

    public SupportTaskLookup()
    {
        _keys = new(() => this.ToDictionary(t => t.Value.SupportTaskReference, t => t.Key));
    }

    public SupportTaskLookup(IDictionary<string, SupportTask> tasks) : base(tasks)
    {
        _keys = new(() => this.ToDictionary(t => t.Value.SupportTaskReference, t => t.Key));
    }

    public string GetKeyFor(string supportTaskRef) => _keys.Value[supportTaskRef];

    public IEnumerable<string> GetKeysFor<T>(ResultPage<T> searchResults)
        where T : ISupportTaskSearchResult
        => searchResults.Select(r => GetKeyFor(r.SupportTaskReference));

    public static SupportTaskLookup Create(Dictionary<string, ISupportTaskCreateResult> results)
         => new(results.ToDictionary(r => r.Key, r => r.Value.SupportTask));

}
