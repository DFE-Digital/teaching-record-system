using System;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.DataStore.Crm
{
    public interface IDataverseAdapter
    {
        Task<CreateTeacherResult> CreateTeacher(CreateTeacherCommand command);

        Task<Account[]> GetIttProviders();

        Task<Contact[]> GetMatchingTeachers(GetTeacherRequest request);

        Task<dfeta_qualification[]> GetQualificationsForTeacher(Guid teacherId, params string[] columnNames);

        Task<Contact> GetTeacher(Guid teacherId, bool resolveMerges = true, params string[] columnNames);

        Task<Contact[]> GetTeachersByTrn(string trn, bool activeOnly = true, params string[] columnNames);

        Task<Contact[]> GetTeachersByTrnAndDoB(string trn, DateOnly birthDate, bool activeOnly = true, params string[] columnNames);

        Task<Contact[]> FindTeachers(FindTeachersQuery query);

        Task<UpdateTeacherResult> UpdateTeacher(UpdateTeacherCommand command);

        Task<Account> GetOrganizationByName(string providerName, params string[] columnNames);

        Task<Account> GetOrganizationByUkprn(string ukprn, params string[] columnNames);

        Task<CrmTask[]> GetCrmTasksForTeacher(Guid teacherId, params string[] columnNames);

        Task<SetIttResultForTeacherResult> SetIttResultForTeacher(
            Guid teacherId,
            string ittProviderUkprn,
            dfeta_ITTResult result,
            DateOnly? assessmentDate);

        Task<bool> UnlockTeacherRecord(Guid teacherId);
    }
}
