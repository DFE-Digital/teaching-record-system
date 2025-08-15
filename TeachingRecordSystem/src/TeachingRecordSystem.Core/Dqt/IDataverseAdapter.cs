#nullable disable

namespace TeachingRecordSystem.Core.Dqt;

public interface IDataverseAdapter
{
    Task<dfeta_qualification[]> GetQualificationsForTeacherAsync(
        Guid teacherId,
        string[] columnNames,
        string[] heQualificationColumnNames = null,
        string[] heSubjectColumnNames = null);

    Task<Contact[]> FindTeachersAsync(FindTeachersQuery query);

    Task<Contact[]> FindTeachersStrictAsync(FindTeachersQuery query);

    Task<Account[]> GetIttProviderOrganizationsByNameAsync(string ukprn, string[] columnNames, bool activeOnly);

    Task<Account[]> GetIttProviderOrganizationsByUkprnAsync(string ukprn, string[] columnNames, bool activeOnly);
}
