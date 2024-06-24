using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactByTrnRequestIdQuery(string TrnRequestId, ColumnSet ColumnSet) : ICrmQuery<Contact?>;
