using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactsByLastNameAndDateOfBirthQuery(string LastName, DateOnly DateOfBirth, ColumnSet ColumnSet) : ICrmQuery<Contact[]>;
