using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveContactsByDateOfBirthQuery(DateOnly DateOfBirth, ContactSearchSortByOption SortBy, int MaxRecordCount, ColumnSet ColumnSet) : ICrmQuery<Contact[]>;
