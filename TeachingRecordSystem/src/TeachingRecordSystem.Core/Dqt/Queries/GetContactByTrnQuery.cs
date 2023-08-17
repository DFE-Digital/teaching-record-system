using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactByTrnQuery(string Trn, ColumnSet ColumnSet) : ICrmQuery<Contact?>;
