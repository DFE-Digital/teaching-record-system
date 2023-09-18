using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactsByNameQuery(string Name, ContactSearchSortByOption SortBy, int MaxRecordCount, ColumnSet ColumnSet) : ICrmQuery<Contact[]>;
