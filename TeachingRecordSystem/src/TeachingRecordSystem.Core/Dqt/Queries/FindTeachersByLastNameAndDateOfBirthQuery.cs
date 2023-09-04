using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record FindTeachersByLastNameAndDateOfBirthQuery(string LastName, DateOnly DateOfBirth, ColumnSet ColumnSet) : ICrmQuery<Contact[]>;
