using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveQtsRegistrationsByContactIdsQuery(IEnumerable<Guid> ContactIds, ColumnSet ColumnSet) : ICrmQuery<IDictionary<Guid, dfeta_qtsregistration[]>>;
