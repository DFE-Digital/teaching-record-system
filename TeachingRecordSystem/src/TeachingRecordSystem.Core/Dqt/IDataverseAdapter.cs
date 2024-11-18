#nullable disable

namespace TeachingRecordSystem.Core.Dqt;

public interface IDataverseAdapter
{
    Task<CreateTeacherResult> CreateTeacherAsync(CreateTeacherCommand command);

    Task<Account[]> GetIttProvidersAsync(bool activeOnly);

    Task<Contact[]> FindTeachersAsync(FindTeachersByTrnBirthDateAndNinoQuery query);

    Task<dfeta_earlyyearsstatus> GetEarlyYearsStatusAsync(Guid earlyYearsStatusId);

    Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacherAsync(
        Guid teacherId,
        string[] columnNames,
        string[] establishmentColumnNames,
        string[] subjectColumnNames,
        string[] qualificationColumnNames,
        bool activeOnly = true);

    Task<dfeta_qualification[]> GetQualificationsForTeacherAsync(
        Guid teacherId,
        string[] columnNames,
        string[] heQualificationColumnNames = null,
        string[] heSubjectColumnNames = null);

    Task<dfeta_qualification> GetQualificationByIdAsync(
        Guid qualificationId,
        string[] columnNames,
        string[] contactColumnNames = null);

    Task<(dfeta_induction, dfeta_inductionperiod[])> GetInductionByTeacherAsync(
        Guid teacherId,
        string[] columnNames,
        string[] inductionPeriodColumnNames = null,
        string[] appropriateBodyColumnNames = null,
        string[] contactColumnNames = null);

    Task<Contact> GetTeacherAsync(Guid teacherId, string[] columnNames, bool resolveMerges = true);

    Task<Contact> GetTeacherByTrnAsync(string trn, string[] columnNames, bool activeOnly = true);

    Task<Contact[]> GetTeachersByTrnAndDoBAsync(string trn, DateOnly birthDate, string[] columnNames, bool activeOnly = true);

    Task<Contact> GetTeacherByTsPersonIdAsync(string tsPersonId, string[] columnNames);

    Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacherAsync(Guid teacherId, string[] columnNames);

    Task<Contact[]> FindTeachersAsync(FindTeachersQuery query);

    Task<Contact[]> FindTeachersStrictAsync(FindTeachersQuery query);

    Task<UpdateTeacherResult> UpdateTeacherAsync(UpdateTeacherCommand command);

    Task UpdateTeacherIdentityInfoAsync(UpdateTeacherIdentityInfoCommand command);

    Task<Account[]> GetIttProviderOrganizationsByNameAsync(string ukprn, string[] columnNames, bool activeOnly);

    Task<Account[]> GetIttProviderOrganizationsByUkprnAsync(string ukprn, string[] columnNames, bool activeOnly);

    Task<Account[]> GetOrganizationsByNameAsync(string providerName, string[] columnNames, bool activeOnly);

    Task<Account[]> GetOrganizationsByUkprnAsync(string ukprn, string[] columnNames);

    Task<CrmTask[]> GetCrmTasksForTeacherAsync(Guid teacherId, string[] columnNames);

    Task<SetIttResultForTeacherResult> SetIttResultForTeacherAsync(
        Guid teacherId,
        string ittProviderUkprn,
        dfeta_ITTResult result,
        DateOnly? assessmentDate,
        string slugId = null);

    Task SetTsPersonIdAsync(Guid teacherId, string tsPersonId);

    Task<dfeta_teacherstatus> GetTeacherStatusAsync(string value, RequestBuilder requestBuilder);

    Task<bool> UnlockTeacherRecordAsync(Guid teacherId);

    Task<SetNpqQualificationResult> SetNpqQualificationAsync(SetNpqQualificationCommand command);

    Task<Contact[]> GetTeachersByHusIdAsync(string husId, string[] columnNames);

    Task<Subject> GetSubjectByTitleAsync(string title);

    Task<Incident[]> GetIncidentsByContactIdAsync(Guid contactId, IncidentState? state, string[] columnNames);

    Task<Contact[]> GetTeachersByInitialTeacherTrainingSlugIdAsync(string slugId, string[] columnNames, RequestBuilder requestBuilder, bool activeOnly = true);

    IAsyncEnumerable<QtsAwardee[]> GetQtsAwardeesForDateRangeAsync(DateTime startDate, DateTime endDate);

    IAsyncEnumerable<InternationalQtsAwardee[]> GetInternationalQtsAwardeesForDateRangeAsync(DateTime startDate, DateTime endDate);

    IAsyncEnumerable<EytsAwardee[]> GetEytsAwardeesForDateRangeAsync(DateTime startDate, DateTime endDate);

    IAsyncEnumerable<InductionCompletee[]> GetInductionCompleteesForDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<Contact[]> GetTeachersBySlugIdAndTrnAsync(string slugId, string trn, string[] columnNames, bool activeOnly = true);

    Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingBySlugIdAsync(string slugId, string[] columnNames, RequestBuilder requestBuilder, bool activeOnly = true);

    Task ClearTeacherIdentityInfoAsync(Guid identityUserId, DateTime updateTimeUtc);

    Task<bool> DoesTeacherHavePendingPIIChangesAsync(Guid teacherId);
}
