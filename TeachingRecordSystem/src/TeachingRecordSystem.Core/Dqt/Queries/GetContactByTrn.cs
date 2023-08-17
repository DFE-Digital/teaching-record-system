using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactByTrn(string Trn, ColumnSet ColumnSet) : ICrmQuery<Contact?>;
