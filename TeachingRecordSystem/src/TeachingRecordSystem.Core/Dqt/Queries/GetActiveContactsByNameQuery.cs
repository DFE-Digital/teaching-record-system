using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveContactsByNameQuery(string Name, ContactSearchSortByOption SortBy, int MaxRecordCount, ColumnSet ColumnSet) : ICrmQuery<Contact[]>;
