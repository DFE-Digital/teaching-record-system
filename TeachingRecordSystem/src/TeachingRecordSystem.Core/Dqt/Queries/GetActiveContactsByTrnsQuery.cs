using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveContactsByTrnsQuery(IEnumerable<string> Trns, ColumnSet ColumnSet) :
    ICrmQuery<IDictionary<string, Contact?>>;
