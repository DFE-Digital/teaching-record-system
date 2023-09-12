using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactsByNameQuery(string Name, int MaxRecordCount, ColumnSet ColumnSet) : ICrmQuery<Contact[]>;

