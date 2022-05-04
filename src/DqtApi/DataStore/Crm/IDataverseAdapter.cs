using System;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.DataStore.Crm
{
    public interface IDataverseAdapter
    {
        Task<CreateTeacherResult> CreateTeacher(CreateTeacherCommand command);

        Task<Account[]> GetIttProviders(bool activeOnly);

        Task<Contact[]> FindTeachers(FindTeachersByTrnBirthDateAndNinoQuery query);

        Task<dfeta_qualification[]> GetQualificationsForTeacher(Guid teacherId, params string[] columnNames);

        Task<Contact> GetTeacher(Guid teacherId, bool resolveMerges = true, params string[] columnNames);

        Task<Contact[]> GetTeachersByTrn(string trn, bool activeOnly = true, params string[] columnNames);

        Task<Contact[]> GetTeachersByTrnAndDoB(string trn, DateOnly birthDate, bool activeOnly = true, params string[] columnNames);

        Task<Contact[]> FindTeachers(FindTeachersQuery query);

        Task<UpdateTeacherResult> UpdateTeacher(UpdateTeacherCommand command);

        Task<Account> GetIttProviderOrganizationByName(string ukprn, bool activeOnly, params string[] columnNames);

        Task<Account> GetIttProviderOrganizationByUkprn(string ukprn, bool activeOnly, params string[] columnNames);

        Task<Account> GetOrganizationByName(string providerName, bool activeOnly, params string[] columnNames);

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
