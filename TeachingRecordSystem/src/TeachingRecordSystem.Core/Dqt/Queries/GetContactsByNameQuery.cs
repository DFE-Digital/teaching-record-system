using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactsByNameQuery(string Name, ColumnSet ColumnSet) : ICrmQuery<Contact[]>;

