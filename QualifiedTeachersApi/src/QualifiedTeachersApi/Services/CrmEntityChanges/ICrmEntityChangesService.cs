using System.Collections.Generic;
using System.Threading;
using Microsoft.Xrm.Sdk;

namespace QualifiedTeachersApi.Services.CrmEntityChanges;

public interface ICrmEntityChangesService
{
    IAsyncEnumerable<IChangedItem[]> GetEntityChanges(
        string key,
        string entityLogicalName,
        int pageSize = 1000,
        CancellationToken cancellationToken = default);
}
