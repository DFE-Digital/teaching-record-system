using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactsByNamesQuery(string[] Names, ColumnSet ColumnSet) : ICrmQuery<Contact[]>;

