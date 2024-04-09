using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveContactByTrnQuery(string Trn, ColumnSet ColumnSet) : ICrmQuery<Contact?>;
