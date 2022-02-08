using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.DataStore.Crm
{
    public interface IDataverseAdapter
    {
        Task<CreateTeacherResult> CreateTeacher(CreateTeacherCommand command);

        Task<IEnumerable<Account>> GetIttProviders();

        Task<IEnumerable<Contact>> GetMatchingTeachersAsync(GetTeacherRequest request);

        Task<IEnumerable<dfeta_qualification>> GetQualificationsAsync(Guid teacherId);

        Task<Contact> GetTeacherAsync(Guid teacherId, bool resolveMerges = true, params string[] columnNames);

        Task<IEnumerable<Contact>> GetTeachersByTrn(string trn, bool activeOnly = true, params string[] columnNames);
        Task<IReadOnlyCollection<Contact>> FindTeachers(FindTeachersQuery query);
        Task<Account> GetOrganizationByProviderName(string providerName, params string[] columnNames);
        Task<Account> GetOrganizationByUkprn(string ukprn, params string[] columnNames);

        Task<SetIttResultForTeacherResult> SetIttResultForTeacher(
            Guid teacherId,
            string ittProviderUkprn,
            dfeta_ITTResult result,
            DateOnly? assessmentDate);

        Task<bool> UnlockTeacherRecordAsync(Guid teacherId);
    }
}
