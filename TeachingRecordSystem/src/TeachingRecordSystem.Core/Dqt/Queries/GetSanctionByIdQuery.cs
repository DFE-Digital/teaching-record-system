using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetSanctionByIdQuery(Guid SanctionId, ColumnSet ColumnSet) : ICrmQuery<dfeta_sanction>;
