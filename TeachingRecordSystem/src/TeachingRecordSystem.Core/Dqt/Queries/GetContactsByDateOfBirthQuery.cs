using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactsByDateOfBirthQuery(DateOnly DateOfBirth, int MaxRecordCount, ColumnSet ColumnSet) : ICrmQuery<Contact[]>;
