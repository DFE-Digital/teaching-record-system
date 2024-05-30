using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactWithMergeResolutionQuery(Guid ContactId, ColumnSet ColumnSet) : ICrmQuery<Contact>;
