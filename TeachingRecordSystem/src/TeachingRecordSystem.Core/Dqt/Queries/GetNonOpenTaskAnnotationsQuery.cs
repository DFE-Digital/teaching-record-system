using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetNonOpenTaskAnnotationsQuery(string[] Subjects, DateTime ModifiedBefore, ColumnSet ColumnSet) : ICrmQuery<Annotation[]>;
