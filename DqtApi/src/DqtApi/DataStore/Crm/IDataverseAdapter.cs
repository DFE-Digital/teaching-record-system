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

        Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(Guid earlyYearsStatusId);

        Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacher(Guid teacherId, string[] columnNames);

        Task<dfeta_qualification[]> GetQualificationsForTeacher(Guid teacherId, string[] columnNames);

        Task<Contact> GetTeacher(Guid teacherId, string[] columnNames, bool resolveMerges = true);

        Task<Contact[]> GetTeachersByTrn(string trn, string[] columnNames, bool activeOnly = true);

        Task<Contact[]> GetTeachersByTrnAndDoB(string trn, DateOnly birthDate, string[] columnNames, bool activeOnly = true);

        Task<Contact> GetTeacherByTsPersonId(string tsPersonId, string[] columnNames);

        Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacher(Guid teacherId, string[] columnNames);

        Task<Contact[]> FindTeachers(FindTeachersQuery query);

        Task<UpdateTeacherResult> UpdateTeacher(UpdateTeacherCommand command);

        Task<Account[]> GetIttProviderOrganizationsByName(string ukprn, string[] columnNames, bool activeOnly);

        Task<Account[]> GetIttProviderOrganizationsByUkprn(string ukprn, string[] columnNames, bool activeOnly);

        Task<Account[]> GetOrganizationsByName(string providerName, string[] columnNames, bool activeOnly);

        Task<Account[]> GetOrganizationsByUkprn(string ukprn, string[] columnNames);

        Task<CrmTask[]> GetCrmTasksForTeacher(Guid teacherId, string[] columnNames);

        Task<SetIttResultForTeacherResult> SetIttResultForTeacher(
            Guid teacherId,
            string ittProviderUkprn,
            dfeta_ITTResult result,
            DateOnly? assessmentDate);

        Task SetTsPersonId(Guid teacherId, string tsPersonId);

        Task<bool> UnlockTeacherRecord(Guid teacherId);

        Task<Contact[]> GetTeachersByHusId(string husId, string[] columnNames);
    }
}
