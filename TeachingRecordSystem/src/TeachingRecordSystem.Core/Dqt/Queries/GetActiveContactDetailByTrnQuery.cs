using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveContactDetailByTrnQuery(string Trn, ColumnSet ColumnSet) : ICrmQuery<ContactDetail?>;
