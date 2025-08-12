#nullable disable

namespace TeachingRecordSystem.Core.Dqt;

public interface IDataverseAdapter
{
    Task<Contact[]> FindTeachersAsync(FindTeachersByTrnBirthDateAndNinoQuery query);

    Task<dfeta_qualification[]> GetQualificationsForTeacherAsync(
        Guid teacherId,
        string[] columnNames,
        string[] heQualificationColumnNames = null,
        string[] heSubjectColumnNames = null);

    Task<Contact> GetTeacherAsync(Guid teacherId, string[] columnNames, bool resolveMerges = true);

    Task<Contact> GetTeacherByTrnAsync(string trn, string[] columnNames, bool activeOnly = true);

    Task<Contact[]> FindTeachersAsync(FindTeachersQuery query);

    Task<Contact[]> FindTeachersStrictAsync(FindTeachersQuery query);

    Task<Account[]> GetIttProviderOrganizationsByNameAsync(string ukprn, string[] columnNames, bool activeOnly);

    Task<Account[]> GetIttProviderOrganizationsByUkprnAsync(string ukprn, string[] columnNames, bool activeOnly);

    Task<dfeta_teacherstatus> GetTeacherStatusAsync(string value, RequestBuilder requestBuilder);

    Task<Incident[]> GetIncidentsByContactIdAsync(Guid contactId, IncidentState? state, string[] columnNames);
}
