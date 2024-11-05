using System.Text;
using Microsoft.Xrm.Sdk.Query;
using Optional;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetPersonCommand(
    string Trn,
    GetPersonCommandIncludes Include,
    DateOnly? DateOfBirth,
    bool ApplyLegacyAlertsBehavior,
    string? NationalInsuranceNumber = null);

[Flags]
public enum GetPersonCommandIncludes
{
    None = 0,
    Induction = 1 << 0,
    InitialTeacherTraining = 1 << 1,
    NpqQualifications = 1 << 2,
    MandatoryQualifications = 1 << 3,
    PendingDetailChanges = 1 << 4,
    HigherEducationQualifications = 1 << 5,
    Sanctions = 1 << 6,
    Alerts = 1 << 7,
    PreviousNames = 1 << 8,
    AllowIdSignInWithProhibitions = 1 << 9,
}

public record GetPersonResult
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Option<bool> PendingNameChange { get; init; }
    public required Option<bool> PendingDateOfBirthChange { get; init; }
    public required string? EmailAddress { get; set; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
    public required Option<GetPersonResultInduction?> Induction { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResultInitialTeacherTraining>> InitialTeacherTraining { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResultNpqQualification>> NpqQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResultMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResultHigherEducationQualification>> HigherEducationQualifications { get; init; }
    public required Option<IReadOnlyCollection<SanctionInfo>> Sanctions { get; init; }
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }
}

public record GetPersonResultInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required SharedModels.InductionStatus? Status { get; init; }
    public required string? StatusDescription { get; init; }
    public required string? CertificateUrl { get; init; }
    public required IReadOnlyCollection<GetPersonResultInductionPeriod> Periods { get; init; }
}

public record GetPersonResultInductionPeriod
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required int? Terms { get; init; }
    public required GetPersonResultInductionPeriodAppropriateBody? AppropriateBody { get; init; }
}

public record GetPersonResultInductionPeriodAppropriateBody
{
    public required string Name { get; init; }
}

public record GetPersonResultInitialTeacherTraining
{
    public required GetPersonResultInitialTeacherTrainingQualification? Qualification { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required IttProgrammeType? ProgrammeType { get; init; }
    public required string? ProgrammeTypeDescription { get; init; }
    public required IttOutcome? Result { get; init; }
    public required GetPersonResultInitialTeacherTrainingAgeRange? AgeRange { get; init; }
    public required GetPersonResultInitialTeacherTrainingProvider? Provider { get; init; }
    public required IReadOnlyCollection<GetPersonResultInitialTeacherTrainingSubject> Subjects { get; init; }
}

public record GetPersonResultInitialTeacherTrainingQualification
{
    public required string Name { get; init; }
}

public record GetPersonResultInitialTeacherTrainingAgeRange
{
    public required string Description { get; init; }
}

public record GetPersonResultInitialTeacherTrainingProvider
{
    public required string Name { get; init; }
    public required string Ukprn { get; init; }
}

public record GetPersonResultInitialTeacherTrainingSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

public record GetPersonResultNpqQualification
{
    public required DateOnly Awarded { get; init; }
    public required GetPersonResultNpqQualificationType Type { get; init; }
    public required string CertificateUrl { get; init; }
}

public record GetPersonResultNpqQualificationType
{
    public required NpqQualificationType Code { get; init; }
    public required string Name { get; init; }
}

public record GetPersonResultMandatoryQualification
{
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }
}

public record GetPersonResultHigherEducationQualification
{
    public required string? Name { get; init; }
    public required DateOnly? Awarded { get; init; }
    public required IReadOnlyCollection<GetPersonResultHigherEducationQualificationSubject> Subjects { get; init; }
}

public record GetPersonResultHigherEducationQualificationSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

public class GetPersonHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    IDataverseAdapter dataverseAdapter,
    PreviousNameHelper previousNameHelper)
{
    public async Task<GetPersonResult?> Handle(GetPersonCommand command)
    {
        var contactDetail = await crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactDetailByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_EYTSDate,
                    Contact.Fields.EMailAddress1,
                    Contact.Fields.dfeta_AllowIDSignInWithProhibitions,
                    Contact.Fields.dfeta_InductionStatus)));

        if (contactDetail is null)
        {
            return null;
        }

        // If a DateOfBirth or NationalInsuranceNumber was provided, ensure the record we've retrieved with the TRN matches
        if (command.DateOfBirth is DateOnly dateOfBirth &&
            contactDetail.Contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false) != dateOfBirth)
        {
            return null;
        }
        if (command.NationalInsuranceNumber is not null)
        {
            // Check the NINO in DQT first. If that fails, check workforce data in TRS (which may have different NINO(s) for the person).

            var normalizedNino = NationalInsuranceNumberHelper.Normalize(command.NationalInsuranceNumber);
            var dqtNino = contactDetail.Contact.dfeta_NINumber;

            if (string.IsNullOrEmpty(dqtNino) || !dqtNino.Equals(normalizedNino, StringComparison.OrdinalIgnoreCase))
            {
                var employmentNinos = await dbContext.TpsEmployments
                    .Where(e => e.PersonId == contactDetail.Contact.Id && e.NationalInsuranceNumber != null)
                    .Select(e => e.NationalInsuranceNumber)
                    .Distinct()
                    .ToArrayAsync();

                if (!employmentNinos.Any(n => n!.Equals(normalizedNino, StringComparison.OrdinalIgnoreCase)))
                {
                    return null;
                }
            }
        }

        // DataverseAdapter operations share an IOrganizationService, which is not thread-safe in our setup.
        // This lock should be used around all calls to DataverseAdatper.
        using var dataverseLock = new SemaphoreSlim(1, 1);

        async Task<T> WithDataverseAdapterLock<T>(Func<Task<T>> action)
        {
            await dataverseLock.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                dataverseLock.Release();
            }
        }

        var contact = contactDetail.Contact;
        var personId = contact.Id;

        var getInductionTask = command.Include.HasFlag(GetPersonCommandIncludes.Induction) ?
            WithDataverseAdapterLock(() => dataverseAdapter.GetInductionByTeacher(
                contact.Id,
                columnNames:
                [
                    dfeta_induction.PrimaryIdAttribute,
                    dfeta_induction.Fields.dfeta_StartDate,
                    dfeta_induction.Fields.dfeta_CompletionDate,
                    dfeta_induction.Fields.dfeta_InductionStatus
                ],
                inductionPeriodColumnNames:
                [
                    dfeta_inductionperiod.Fields.dfeta_InductionId,
                    dfeta_inductionperiod.Fields.dfeta_StartDate,
                    dfeta_inductionperiod.Fields.dfeta_EndDate,
                    dfeta_inductionperiod.Fields.dfeta_Numberofterms,
                    dfeta_inductionperiod.Fields.dfeta_AppropriateBodyId
                ],
                appropriateBodyColumnNames:
                [
                    Account.PrimaryIdAttribute,
                    Account.Fields.Name
                ])) :
            null;

        var getIttTask = command.Include.HasFlag(GetPersonCommandIncludes.InitialTeacherTraining) ?
            WithDataverseAdapterLock(() => dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                contact.Id,
                columnNames:
                [
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.dfeta_TraineeID,
                    dfeta_initialteachertraining.Fields.StateCode
                ],
                establishmentColumnNames:
                [
                    Account.PrimaryIdAttribute,
                    Account.Fields.dfeta_UKPRN,
                    Account.Fields.Name
                ],
                subjectColumnNames:
                [
                    dfeta_ittsubject.PrimaryIdAttribute,
                    dfeta_ittsubject.Fields.dfeta_name,
                    dfeta_ittsubject.Fields.dfeta_Value
                ],
                qualificationColumnNames:
                [
                    dfeta_ittqualification.PrimaryIdAttribute,
                    dfeta_ittqualification.Fields.dfeta_name
                ],
                activeOnly: true)) :
            null;

        var getMqsTask = command.Include.HasFlag(GetPersonCommandIncludes.MandatoryQualifications) ?
            dbContext.MandatoryQualifications.Where(q => q.PersonId == personId).ToArrayAsync() :
            null;

        var getQualificationsTask = (command.Include & (GetPersonCommandIncludes.NpqQualifications | GetPersonCommandIncludes.HigherEducationQualifications)) != 0 ?
            crmQueryDispatcher.ExecuteQuery(
                new GetQualificationsByContactIdQuery(
                    contact.Id,
                    new ColumnSet(
                        dfeta_qualification.PrimaryIdAttribute,
                        dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                        dfeta_qualification.Fields.dfeta_Type,
                        dfeta_qualification.Fields.StateCode),
                    IncludeHigherEducationDetails: command.Include.HasFlag(GetPersonCommandIncludes.HigherEducationQualifications))) :
            null;

        async Task<(bool PendingNameRequest, bool PendingDateOfBirthRequest)> GetPendingDetailChanges()
        {
            var nameChangeSubject = await referenceDataCache.GetSubjectByTitle("Change of Name");
            var dateOfBirthChangeSubject = await referenceDataCache.GetSubjectByTitle("Change of Date of Birth");

            var incidents = await WithDataverseAdapterLock(() => dataverseAdapter.GetIncidentsByContactId(
                contact.Id,
                IncidentState.Active,
                columnNames: [Incident.Fields.SubjectId, Incident.Fields.StateCode]));

            var pendingNameChange = incidents.Any(i => i.SubjectId.Id == nameChangeSubject.Id);
            var pendingDateOfBirthChange = incidents.Any(i => i.SubjectId.Id == dateOfBirthChangeSubject.Id);

            return (pendingNameChange, pendingDateOfBirthChange);
        }

        var getPendingDetailChangesTask = command.Include.HasFlag(GetPersonCommandIncludes.PendingDetailChanges) ?
            GetPendingDetailChanges() :
            null;

        async Task<SanctionResult[]> GetSanctions()
        {
            var result = await crmQueryDispatcher.ExecuteQuery(new GetSanctionsByContactIdsQuery(
                [contact.Id],
                ActiveOnly: true,
                ColumnSet: new(
                    dfeta_sanction.Fields.dfeta_StartDate,
                    dfeta_sanction.Fields.dfeta_EndDate,
                    dfeta_sanction.Fields.dfeta_SanctionDetails,
                    dfeta_sanction.Fields.dfeta_Spent)));

            return result[contact.Id];
        }

        var getSanctionsTask = command.Include.HasFlag(GetPersonCommandIncludes.Sanctions) || command.Include.HasFlag(GetPersonCommandIncludes.Alerts) ?
            GetSanctions() :
            null;

        IEnumerable<NameInfo>? previousNames = previousNameHelper.GetFullPreviousNames(contactDetail.PreviousNames, contactDetail.Contact)
            .Select(name => new NameInfo()
            {
                FirstName = name.FirstName,
                MiddleName = name.MiddleName,
                LastName = name.LastName
            })
            .AsReadOnly();

        var firstName = contact.ResolveFirstName();
        var middleName = contact.ResolveMiddleName();
        var lastName = contact.ResolveLastName();

        var qtsRegistrations = await WithDataverseAdapterLock(() => dataverseAdapter.GetQtsRegistrationsByTeacher(
            contact.Id,
            columnNames:
            [
                dfeta_qtsregistration.Fields.dfeta_QTSDate,
                dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                dfeta_qtsregistration.Fields.dfeta_name,
                dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId
            ]));

        var qts = qtsRegistrations.OrderByDescending(x => x.CreatedOn).FirstOrDefault(qts => qts.dfeta_QTSDate is not null);
        var eyts = qtsRegistrations.OrderByDescending(x => x.CreatedOn).FirstOrDefault(qts => qts.dfeta_EYTSDate is not null);
        var allTeacherStatuses = await referenceDataCache.GetTeacherStatuses();
        var allEarlyYearsStatuses = await referenceDataCache.GetEytsStatuses();
        var eytsStatus = eyts is not null ? allEarlyYearsStatuses.Single(x => x.Id == eyts.dfeta_EarlyYearsStatusId.Id) : null;
        var qtsStatus = qts is not null ? allTeacherStatuses.Single(x => x.Id == qts.dfeta_TeacherStatusId.Id) : null;

        var allowIdSignInWithProhibitions = command.Include.HasFlag(GetPersonCommandIncludes.AllowIdSignInWithProhibitions) ?
            Option.Some(contact.dfeta_AllowIDSignInWithProhibitions == true) :
            default;

        return new GetPersonResult()
        {
            Trn = command.Trn,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = contact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            NationalInsuranceNumber = contact.dfeta_NINumber,
            PendingNameChange = command.Include.HasFlag(GetPersonCommandIncludes.PendingDetailChanges) ? Option.Some((await getPendingDetailChangesTask!).PendingNameRequest) : default,
            PendingDateOfBirthChange = command.Include.HasFlag(GetPersonCommandIncludes.PendingDetailChanges) ? Option.Some((await getPendingDetailChangesTask!).PendingDateOfBirthRequest) : default,
            Qts = await QtsInfo.Create(qts, referenceDataCache),
            Eyts = await EytsInfo.Create(eyts, referenceDataCache),
            EmailAddress = contact.EMailAddress1,
            Induction = command.Include.HasFlag(GetPersonCommandIncludes.Induction) ?
                Option.Some(MapInduction(await getInductionTask!, contact)) :
                default,
            InitialTeacherTraining = command.Include.HasFlag(GetPersonCommandIncludes.InitialTeacherTraining) ?
                Option.Some((await getIttTask!)
                    .Select(i => new GetPersonResultInitialTeacherTraining()
                    {
                        Qualification = MapIttQualification(i),
                        ProgrammeType = i.dfeta_ProgrammeType?.ConvertToEnumByValue<dfeta_ITTProgrammeType, IttProgrammeType>(),
                        ProgrammeTypeDescription = i.dfeta_ProgrammeType?.ConvertToEnumByValue<dfeta_ITTProgrammeType, IttProgrammeType>().GetDescription(),
                        StartDate = i.dfeta_ProgrammeStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                        EndDate = i.dfeta_ProgrammeEndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                        Result = i.dfeta_Result.HasValue ? i.dfeta_Result.Value.ConvertFromITTResult() : null,
                        AgeRange = MapAgeRange(i.dfeta_AgeRangeFrom, i.dfeta_AgeRangeTo),
                        Provider = MapIttProvider(i),
                        Subjects = MapSubjects(i)
                    })
                    .OrderByDescending(i => i.StartDate)
                    .AsReadOnly()) :
                default,
            NpqQualifications = command.Include.HasFlag(GetPersonCommandIncludes.NpqQualifications) ?
                Option.Some(MapNpqQualifications(await getQualificationsTask!)) :
                default,
            MandatoryQualifications = command.Include.HasFlag(GetPersonCommandIncludes.MandatoryQualifications) ?
                Option.Some(MapMandatoryQualifications((await getMqsTask!))) :
                default,
            HigherEducationQualifications = command.Include.HasFlag(GetPersonCommandIncludes.HigherEducationQualifications) ?
                Option.Some(MapHigherEducationQualifications((await getQualificationsTask!))) :
                default,
            Sanctions = command.Include.HasFlag(GetPersonCommandIncludes.Sanctions) ?
                Option.Some((await getSanctionsTask!)
                    .Where(s => Constants.LegacyExposableSanctionCodes.Contains(s.SanctionCode))
                    .Where(s => s.Sanction.dfeta_EndDate is null && s.Sanction.dfeta_Spent != true)
                    .Select(s => new SanctionInfo()
                    {
                        Code = s.SanctionCode,
                        StartDate = s.Sanction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                    })
                    .AsReadOnly()) :
                default,
            Alerts = command.Include.HasFlag(GetPersonCommandIncludes.Alerts) ?
                Option.Some(await (await getSanctionsTask!)
                    .ToAsyncEnumerable()
                    .WhereAwait(async s =>
                    {
                        var alertType = await referenceDataCache.GetAlertTypeByDqtSanctionCodeIfExists(s.SanctionCode);

                        if (alertType is null)
                        {
                            return false;
                        }

                        // The Legacy behavior is to only return prohibition-type alerts
                        if (command.ApplyLegacyAlertsBehavior)
                        {
                            return Constants.LegacyProhibitionSanctionCodes.Contains(s.SanctionCode);
                        }

                        return !alertType.InternalOnly;
                    })
                    .SelectAwait(async s =>
                    {
                        var alertType = await referenceDataCache.GetAlertTypeByDqtSanctionCode(s.SanctionCode);
                        var alertCategory = await referenceDataCache.GetAlertCategoryById(alertType.AlertCategoryId);

                        return new Alert()
                        {
                            AlertId = s.Sanction.Id,
                            AlertType = new()
                            {
                                AlertTypeId = alertType.AlertTypeId,
                                AlertCategory = new()
                                {
                                    AlertCategoryId = alertCategory.AlertCategoryId,
                                    Name = alertCategory.Name
                                },
                                Name = alertType.Name,
                                DqtSanctionCode = alertType.DqtSanctionCode!
                            },
                            Details = s.Sanction.dfeta_SanctionDetails,
                            StartDate = s.Sanction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            EndDate = s.Sanction.dfeta_EndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                        };
                    })
                    .AsReadOnlyAsync()) :
                default,
            PreviousNames = command.Include.HasFlag(GetPersonCommandIncludes.PreviousNames) ?
                Option.Some(previousNames.Select(n => n).AsReadOnly()) :
                default,
            AllowIdSignInWithProhibitions = allowIdSignInWithProhibitions
        };
    }

    private static GetPersonResultInduction? MapInduction((dfeta_induction Induction, dfeta_inductionperiod[] Inductionperiods) data, TeachingRecordSystem.Core.Dqt.Models.Contact contact)
    {
        var inductionStatus = contact.dfeta_InductionStatus?.ConvertToInductionStatus();
        return data.Induction != null ?
            new GetPersonResultInduction()
            {
                StartDate = data.Induction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                EndDate = data.Induction.dfeta_CompletionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                Status = inductionStatus,
                StatusDescription = contact.dfeta_InductionStatus?.GetDescription(),
                CertificateUrl =
                    (data.Induction.dfeta_InductionStatus == dfeta_InductionStatus.Pass || data.Induction.dfeta_InductionStatus == dfeta_InductionStatus.PassedinWales) &&
                        data.Induction.dfeta_CompletionDate is not null ?
                    "/v3/certificates/induction" :
                    null,
                Periods = data.Inductionperiods.Select(MapInductionPeriod).ToArray()
            } :
            inductionStatus.HasValue ?
                    new GetPersonResultInduction()
                    {
                        StartDate = null,
                        EndDate = null,
                        Status = inductionStatus,
                        StatusDescription = contact.dfeta_InductionStatus?.GetDescription(),
                        CertificateUrl = null,
                        Periods = Array.Empty<GetPersonResultInductionPeriod>()
                    } :
            null;
    }

    private static GetPersonResultInductionPeriod MapInductionPeriod(dfeta_inductionperiod inductionPeriod)
    {
        var appropriateBody = inductionPeriod.Extract<Account>("appropriatebody", Account.PrimaryIdAttribute);

        return new GetPersonResultInductionPeriod()
        {
            StartDate = inductionPeriod.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EndDate = inductionPeriod.dfeta_EndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            Terms = inductionPeriod.dfeta_Numberofterms,
            AppropriateBody = appropriateBody is not null ?
                new GetPersonResultInductionPeriodAppropriateBody()
                {
                    Name = appropriateBody.Name
                } :
                null
        };
    }

    private static GetPersonResultInitialTeacherTrainingQualification? MapIttQualification(dfeta_initialteachertraining initialTeacherTraining)
    {
        var qualification = initialTeacherTraining.Extract<dfeta_ittqualification>("qualification", dfeta_ittqualification.PrimaryIdAttribute);

        return qualification != null ?
            new GetPersonResultInitialTeacherTrainingQualification()
            {
                Name = qualification.dfeta_name
            } :
            null;
    }

    private static GetPersonResultInitialTeacherTrainingAgeRange? MapAgeRange(dfeta_AgeRange? ageRangeFrom, dfeta_AgeRange? ageRangeTo)
    {
        var ageRangeDescription = new StringBuilder();
        var ageRangeFromName = ageRangeFrom.HasValue ? ageRangeFrom.Value.GetMetadata().Name : null;
        var ageRangeToName = ageRangeTo.HasValue ? ageRangeTo.Value.GetMetadata().Name : null;

        if (ageRangeFromName != null)
        {
            ageRangeDescription.AppendFormat("{0} ", ageRangeFromName);
        }

        if (ageRangeToName != null)
        {
            ageRangeDescription.AppendFormat("to {0} ", ageRangeToName);
        }

        if (ageRangeDescription.Length > 0)
        {
            ageRangeDescription.Append("years");
        }

        return ageRangeDescription.Length > 0 ?
            new GetPersonResultInitialTeacherTrainingAgeRange()
            {
                Description = ageRangeDescription.ToString()
            } :
            null;
    }

    private static GetPersonResultInitialTeacherTrainingProvider? MapIttProvider(dfeta_initialteachertraining initialTeacherTraining)
    {
        var establishment = initialTeacherTraining.Extract<Account>("establishment", Account.PrimaryIdAttribute);

        return establishment != null ?
            new GetPersonResultInitialTeacherTrainingProvider()
            {
                Name = establishment.Name,
                Ukprn = establishment.dfeta_UKPRN
            } :
            null;
    }

    private static IReadOnlyCollection<GetPersonResultInitialTeacherTrainingSubject> MapSubjects(dfeta_initialteachertraining initialTeacherTraining)
    {
        var subjects = new List<GetPersonResultInitialTeacherTrainingSubject>();

        for (var index = 1; index <= 3; index++)
        {
            var subject = initialTeacherTraining.Extract<dfeta_ittsubject>($"subject{index}", dfeta_ittsubject.PrimaryIdAttribute);

            if (subject is not null)
            {
                subjects.Add(new GetPersonResultInitialTeacherTrainingSubject()
                {
                    Code = subject.dfeta_Value,
                    Name = subject.dfeta_name
                });
            }
        }

        return subjects;
    }

    private static IReadOnlyCollection<GetPersonResultNpqQualification> MapNpqQualifications(dfeta_qualification[] qualifications) =>
        qualifications
            .Where(q => q.dfeta_Type.HasValue
                && q.dfeta_Type.Value.IsNpq()
                && q.StateCode == dfeta_qualificationState.Active
                && q.dfeta_CompletionorAwardDate.HasValue)
            .Select(q => new GetPersonResultNpqQualification()
            {
                Awarded = q.dfeta_CompletionorAwardDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                Type = new GetPersonResultNpqQualificationType()
                {
                    Code = MapNpqQualificationType(q.dfeta_Type!.Value),
                    Name = q.dfeta_Type.Value.GetName(),
                },
                CertificateUrl = $"/v3/certificates/npq/{q.Id}"
            })
            .ToArray();

    private static NpqQualificationType MapNpqQualificationType(dfeta_qualification_dfeta_Type qualificationType) =>
        qualificationType switch
        {
            dfeta_qualification_dfeta_Type.NPQEL => NpqQualificationType.NPQEL,
            dfeta_qualification_dfeta_Type.NPQEYL => NpqQualificationType.NPQEYL,
            dfeta_qualification_dfeta_Type.NPQH => NpqQualificationType.NPQH,
            dfeta_qualification_dfeta_Type.NPQLBC => NpqQualificationType.NPQLBC,
            dfeta_qualification_dfeta_Type.NPQLL => NpqQualificationType.NPQLL,
            dfeta_qualification_dfeta_Type.NPQLT => NpqQualificationType.NPQLT,
            dfeta_qualification_dfeta_Type.NPQLTD => NpqQualificationType.NPQLTD,
            dfeta_qualification_dfeta_Type.NPQML => NpqQualificationType.NPQML,
            dfeta_qualification_dfeta_Type.NPQSL => NpqQualificationType.NPQSL,
            _ => throw new ArgumentException($"Unrecognized qualification type: '{qualificationType}'.", nameof(qualificationType))
        };

    private static IReadOnlyCollection<GetPersonResultMandatoryQualification> MapMandatoryQualifications(PostgresModels.MandatoryQualification[] qualifications) =>
        qualifications
            .Where(q => q.EndDate.HasValue && q.Specialism.HasValue)
            .Select(mq => new GetPersonResultMandatoryQualification()
            {
                Awarded = mq.EndDate!.Value,
                Specialism = mq.Specialism!.Value.GetTitle()
            })
            .ToArray();

    private static IReadOnlyCollection<GetPersonResultHigherEducationQualification> MapHigherEducationQualifications(dfeta_qualification[] qualifications) =>
        qualifications
            .Where(q =>
                q.dfeta_Type.HasValue &&
                q.dfeta_Type.Value == dfeta_qualification_dfeta_Type.HigherEducation &&
                q.StateCode == dfeta_qualificationState.Active)
            .Select(q =>
            {
                var heQualification = q.Extract<dfeta_hequalification>();

                var heSubject1 = q.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}1", dfeta_hesubject.PrimaryIdAttribute);
                var heSubject2 = q.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}2", dfeta_hesubject.PrimaryIdAttribute);
                var heSubject3 = q.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}3", dfeta_hesubject.PrimaryIdAttribute);

                var heSubjects = new List<GetPersonResultHigherEducationQualificationSubject>();

                if (heSubject1 != null)
                {
                    heSubjects.Add(new GetPersonResultHigherEducationQualificationSubject()
                    {
                        Code = heSubject1.dfeta_Value,
                        Name = heSubject1.dfeta_name,
                    });
                }

                if (heSubject2 != null)
                {
                    heSubjects.Add(new GetPersonResultHigherEducationQualificationSubject()
                    {
                        Code = heSubject2.dfeta_Value,
                        Name = heSubject2.dfeta_name,
                    });
                }

                if (heSubject3 != null)
                {
                    heSubjects.Add(new GetPersonResultHigherEducationQualificationSubject()
                    {
                        Code = heSubject3.dfeta_Value,
                        Name = heSubject3.dfeta_name,
                    });
                }

                return new GetPersonResultHigherEducationQualification()
                {
                    Name = heQualification?.dfeta_name,
                    Awarded = q.dfeta_CompletionorAwardDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    Subjects = heSubjects.ToArray()
                };
            })
            .ToArray();
}
