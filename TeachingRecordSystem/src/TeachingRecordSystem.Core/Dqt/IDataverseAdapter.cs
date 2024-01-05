#nullable disable

namespace TeachingRecordSystem.Core.Dqt;

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
        string[] heSubjectColumnNames = null);

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

    Task<Contact[]> FindTeachersStrict(FindTeachersQuery query);

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
        DateOnly? assessmentDate,
        string slugId = null);

    Task SetTsPersonId(Guid teacherId, string tsPersonId);

    Task<dfeta_teacherstatus> GetTeacherStatus(string value, RequestBuilder requestBuilder);

    Task<bool> UnlockTeacherRecord(Guid teacherId);

    Task<SetNpqQualificationResult> SetNpqQualification(SetNpqQualificationCommand command);

    Task<Contact[]> GetTeachersByHusId(string husId, string[] columnNames);

    Task<Subject> GetSubjectByTitle(string title);

    Task<Incident[]> GetIncidentsByContactId(Guid contactId, IncidentState? state, string[] columnNames);

    Task<Contact[]> GetTeachersByInitialTeacherTrainingSlugId(string slugId, string[] columnNames, RequestBuilder requestBuilder, bool activeOnly = true);

    IAsyncEnumerable<QtsAwardee[]> GetQtsAwardeesForDateRange(DateTime startDate, DateTime endDate);

    IAsyncEnumerable<InternationalQtsAwardee[]> GetInternationalQtsAwardeesForDateRange(DateTime startDate, DateTime endDate);

    IAsyncEnumerable<EytsAwardee[]> GetEytsAwardeesForDateRange(DateTime startDate, DateTime endDate);

    IAsyncEnumerable<InductionCompletee[]> GetInductionCompleteesForDateRange(DateTime startDate, DateTime endDate);

    Task<Contact[]> GetTeachersBySlugIdAndTrn(string slugId, string trn, string[] columnNames, bool activeOnly = true);

    Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingBySlugId(string slugId, string[] columnNames, RequestBuilder requestBuilder, bool activeOnly = true);

    Task ClearTeacherIdentityInfo(Guid identityUserId, DateTime updateTimeUtc);

    Task<bool> DoesTeacherHavePendingPIIChanges(Guid teacherId);
}
