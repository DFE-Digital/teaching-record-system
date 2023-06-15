#nullable disable
using Microsoft.Xrm.Sdk.Metadata;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using static TeachingRecordSystem.Api.DataStore.Crm.DataverseAdapter;

namespace TeachingRecordSystem.Api.DataStore.Crm;

public interface IDataverseAdapter
{
    Task<CreateTeacherResult> CreateTeacher(CreateTeacherCommand command);

    Task<Account[]> GetIttProviders(bool activeOnly);

    Task<Contact[]> FindTeachers(FindTeachersByTrnBirthDateAndNinoQuery query);

    Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(Guid earlyYearsStatusId);

    Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacher(
        Guid teacherId,
        string[] columnNames,
        string[] establishmentColumnNames,
        string[] subjectColumnNames,
        string[] qualificationColumnNames,
        bool activeOnly = true);

    Task<dfeta_qualification[]> GetQualificationsForTeacher(
        Guid teacherId,
        string[] columnNames,
        string[] heQualificationColumnNames = null,
        string[] heSubjectColumnNames = null,
        string[] specialismColumnNames = null);

    Task<dfeta_qualification> GetQualificationById(
        Guid qualificationId,
        string[] columnNames,
        string[] contactColumnNames = null);

    Task<(dfeta_induction, dfeta_inductionperiod[])> GetInductionByTeacher(
        Guid teacherId,
        string[] columnNames,
        string[] inductionPeriodColumnNames = null,
        string[] appropriateBodyColumnNames = null,
        string[] contactColumnNames = null);

    Task<Contact> GetTeacher(Guid teacherId, string[] columnNames, bool resolveMerges = true);

    Task<Contact> GetTeacherByTrn(string trn, string[] columnNames, bool activeOnly = true);

    Task<Contact[]> GetTeachersByTrnAndDoB(string trn, DateOnly birthDate, string[] columnNames, bool activeOnly = true);

    Task<Contact> GetTeacherByTsPersonId(string tsPersonId, string[] columnNames);

    Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacher(Guid teacherId, string[] columnNames);

    Task<Contact[]> FindTeachers(FindTeachersQuery query);

    Task<Contact[]> FindTeachersByLastNameAndDateOfBirth(string lastName, DateOnly dateOfBirth, string[] columnNames);

    Task<UpdateTeacherResult> UpdateTeacher(UpdateTeacherCommand command);

    Task UpdateTeacherIdentityInfo(UpdateTeacherIdentityInfoCommand command);

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

    Task<SetNpqQualificationResult> SetNpqQualification(SetNpqQualificationCommand command);

    Task<Contact[]> GetTeachersByHusId(string husId, string[] columnNames);

    Task<Guid> CreateNameChangeIncident(CreateNameChangeIncidentCommand command);

    Task<Guid> CreateDateOfBirthChangeIncident(CreateDateOfBirthChangeIncidentCommand command);

    Task<Subject> GetSubjectByTitle(string title, string[] columnNames);

    Task<Incident[]> GetIncidentsByContactId(Guid contactId, IncidentState? state, string[] columnNames);

    Task<EntityMetadata> GetEntityMetadata(string entityLogicalName, EntityFilters entityFilters = EntityFilters.Default);

    Task<Contact[]> GetTeachersByInitialTeacherTrainingSlugId(string slugId, string[] columnNames, RequestBuilder requestBuilder, bool activeOnly = true);

    Task<QtsAwardee[]> GetQtsAwardeesForDateRange(DateTime startDate, DateTime endDate);

    Task<InternationalQtsAwardee[]> GetInternationalQtsAwardeesForDateRange(DateTime startDate, DateTime endDate);

    Task<EytsAwardee[]> GetEytsAwardeesForDateRange(DateTime startDate, DateTime endDate);

    Task<InductionCompletee[]> GetInductionCompleteesForDateRange(DateTime startDate, DateTime endDate);
}
