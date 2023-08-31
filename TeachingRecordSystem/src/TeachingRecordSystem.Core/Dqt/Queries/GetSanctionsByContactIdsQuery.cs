using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetSanctionsByContactIdsQuery(
    IEnumerable<Guid> ContactIds,
    bool ActiveOnly,
    ColumnSet ColumnSet) :
    ICrmQuery<IDictionary<Guid, SanctionResult[]>>;

public record SanctionResult(dfeta_sanction Sanction, string SanctionCode);
