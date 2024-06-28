using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetResolvedIncidentAnnotationsQuery(Guid[] SubjectIds, DateTime ModifiedBefore, ColumnSet ColumnSet) : ICrmQuery<Annotation[]>;
