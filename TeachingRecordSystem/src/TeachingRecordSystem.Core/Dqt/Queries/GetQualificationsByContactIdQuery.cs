using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetQualificationsByContactIdQuery(Guid ContactId, ColumnSet ColumnSet, bool IncludeHigherEducationDetails = false) : ICrmQuery<dfeta_qualification[]>;
