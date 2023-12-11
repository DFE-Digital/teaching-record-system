using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Services.CrmEntityChanges;

public interface ICrmEntityChangesService
{
    IAsyncEnumerable<IChangedItem[]> GetEntityChanges(
        string changesKey,
        string entityLogicalName,
        ColumnSet columns,
        DateTime? modifiedSince,
        int pageSize = 1000,
        bool rollUpChanges = true,
        CancellationToken cancellationToken = default);
}
