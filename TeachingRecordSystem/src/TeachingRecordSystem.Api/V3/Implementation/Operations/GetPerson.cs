using System.Text;
using Microsoft.Xrm.Sdk.Query;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using IttResult = OneOf.OneOf<TeachingRecordSystem.Api.V3.Implementation.Operations.GetPersonResultInitialTeacherTraining, TeachingRecordSystem.Api.V3.Implementation.Operations.GetPersonResultInitialTeacherTrainingForAppropriateBody>;
using QtlsStatus = TeachingRecordSystem.Api.V3.Implementation.Dtos.QtlsStatus;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetPersonCommand(
    string Trn,
    GetPersonCommandIncludes Include,
    DateOnly? DateOfBirth,
    bool ApplyLegacyAlertsBehavior,
    bool ApplyAppropriateBodyUserRestrictions,
    string? NationalInsuranceNumber = null);

[Flags]
public enum GetPersonCommandIncludes
{
    None = 0,
    Induction = 1 << 0,
    InitialTeacherTraining = 1 << 1,
    MandatoryQualifications = 1 << 3,
    PendingDetailChanges = 1 << 4,
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
    public required Option<GetPersonResultInduction> Induction { get; init; }
    public required Option<GetPersonResultDqtInduction?> DqtInduction { get; init; }
    public required Option<IReadOnlyCollection<IttResult>> InitialTeacherTraining { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResultMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<SanctionInfo>> Sanctions { get; init; }
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }
    public required QtlsStatus QtlsStatus { get; set; }
}

public record GetPersonResultInduction : InductionInfo
{
    public required string? CertificateUrl { get; init; }
}

public record GetPersonResultDqtInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required DqtInductionStatus Status { get; init; }
    public required string? StatusDescription { get; init; }
    public required string? CertificateUrl { get; init; }
    public required IReadOnlyCollection<GetPersonResultDqtInductionPeriod> Periods { get; init; }
}

public record GetPersonResultDqtInductionPeriod
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

/// <summary>
/// The subset of <see cref="GetPersonResultInitialTeacherTraining"/> that contains only the information
/// AppropriateBody users are permitted to see.
/// </summary>
public record GetPersonResultInitialTeacherTrainingForAppropriateBody
{
    public required GetPersonResultInitialTeacherTrainingProvider Provider { get; init; }
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

public record GetPersonResultMandatoryQualification
{
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }
}

public class GetPersonHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    IDataverseAdapter dataverseAdapter,
    PreviousNameHelper previousNameHelper)
{
    public async Task<ApiResult<GetPersonResult>> HandleAsync(GetPersonCommand command)
    {
        if (command.ApplyAppropriateBodyUserRestrictions)
        {
            if ((command.Include & ~(GetPersonCommandIncludes.Induction | GetPersonCommandIncludes.Alerts |
                GetPersonCommandIncludes.InitialTeacherTraining)) != 0)
            {
                return ApiError.ForbiddenForAppropriateBody();
            }

            if (command.DateOfBirth is null)
            {
                return ApiError.ForbiddenForAppropriateBody();
            }
        }

        var contactDetail = await crmQueryDispatcher.ExecuteQueryAsync(
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
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_QtlsDateHasBeenSet,
                    Contact.Fields.dfeta_qtlsdate)));

        if (contactDetail is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        // If a DateOfBirth or NationalInsuranceNumber was provided, ensure the record we've retrieved with the TRN matches
        if (command.DateOfBirth is DateOnly dateOfBirth &&
            contactDetail.Contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false) != dateOfBirth)
        {
            return ApiError.PersonNotFound(command.Trn, dateOfBirth: dateOfBirth);
        }
        if (command.NationalInsuranceNumber is not null)
        {
            // Check the NINO in DQT first. If that fails, check workforce data in TRS (which may have different NINO(s) for the person).

            var normalizedNino = NationalInsuranceNumber.Normalize(command.NationalInsuranceNumber);
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
                    return ApiError.PersonNotFound(command.Trn, nationalInsuranceNumber: command.NationalInsuranceNumber);
                }
            }
        }

        // DataverseAdapter operations share an IOrganizationService, which is not thread-safe in our setup.
        // This lock should be used around all calls to DataverseAdatper.
        using var dataverseLock = new SemaphoreSlim(1, 1);

        async Task<T> WithDataverseAdapterLockAsync<T>(Func<Task<T>> action)
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

        using var trsDbLock = new SemaphoreSlim(1, 1);

        async Task<T> WithTrsDbLockAsync<T>(Func<Task<T>> action)
        {
            await trsDbLock.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                trsDbLock.Release();
            }
        }

        var contact = contactDetail.Contact;
        var personId = contact.Id;

        var getPersonTask = command.Include.HasFlag(GetPersonCommandIncludes.Induction)
            ? WithTrsDbLockAsync(() => dbContext.Persons.SingleAsync(p => p.PersonId == personId))
            : null;

        var getIttTask = command.Include.HasFlag(GetPersonCommandIncludes.InitialTeacherTraining) ?
            WithDataverseAdapterLockAsync(() => dataverseAdapter.GetInitialTeacherTrainingByTeacherAsync(
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
            WithTrsDbLockAsync(() => dbContext.MandatoryQualifications.Where(q => q.PersonId == personId).ToArrayAsync()) :
            null;

        async Task<(bool PendingNameRequest, bool PendingDateOfBirthRequest)> GetPendingDetailChangesAsync()
        {
            var nameChangeSubject = await referenceDataCache.GetSubjectByTitleAsync("Change of Name");
            var dateOfBirthChangeSubject = await referenceDataCache.GetSubjectByTitleAsync("Change of Date of Birth");

            var incidents = await WithDataverseAdapterLockAsync(() => dataverseAdapter.GetIncidentsByContactIdAsync(
                contact.Id,
                IncidentState.Active,
                columnNames: [Incident.Fields.SubjectId, Incident.Fields.StateCode]));

            var pendingNameChange = incidents.Any(i => i.SubjectId.Id == nameChangeSubject.Id);
            var pendingDateOfBirthChange = incidents.Any(i => i.SubjectId.Id == dateOfBirthChangeSubject.Id);

            return (pendingNameChange, pendingDateOfBirthChange);
        }

        var getPendingDetailChangesTask = command.Include.HasFlag(GetPersonCommandIncludes.PendingDetailChanges) ?
            GetPendingDetailChangesAsync() :
            null;

        var getAlertsTask = command.Include.HasFlag(GetPersonCommandIncludes.Sanctions) || command.Include.HasFlag(GetPersonCommandIncludes.Alerts) ?
            WithTrsDbLockAsync(() => dbContext.Alerts.Where(a => a.PersonId == contact.Id).ToArrayAsync()) :
            null;

        IEnumerable<NameInfo> previousNames = previousNameHelper.GetFullPreviousNames(contactDetail.PreviousNames, contactDetail.Contact)
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

        var qtsRegistrations = (await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveQtsRegistrationsByContactIdsQuery(
                [contact.Id],
                new ColumnSet(
                    dfeta_qtsregistration.Fields.dfeta_QTSDate,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                    dfeta_qtsregistration.Fields.dfeta_name,
                    dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                    dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                    dfeta_qtsregistration.Fields.dfeta_PersonId))))[contact.Id];

        var eyts = qtsRegistrations.OrderByDescending(x => x.CreatedOn).FirstOrDefault(qts => qts.dfeta_EYTSDate is not null);

        var allowIdSignInWithProhibitions = command.Include.HasFlag(GetPersonCommandIncludes.AllowIdSignInWithProhibitions) ?
            Option.Some(contact.dfeta_AllowIDSignInWithProhibitions == true) :
            default;

        Option<GetPersonResultDqtInduction?> dqtInduction = default;
        Option<GetPersonResultInduction> induction = default;
        if (command.Include.HasFlag(GetPersonCommandIncludes.Induction))
        {
            var mappedInduction = MapInduction((await getPersonTask!));
            dqtInduction = Option.Some(mappedInduction.DqtInduction);
            induction = Option.Some(mappedInduction.Induction);
        }

        var qtlsStatus = MapQtlsStatus(contact.dfeta_qtlsdate, contact.dfeta_QtlsDateHasBeenSet);

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
            Qts = await QtsInfo.CreateAsync(qtsRegistrations, contact.dfeta_qtlsdate, referenceDataCache),
            QtlsStatus = qtlsStatus,
            Eyts = await EytsInfo.CreateAsync(eyts, referenceDataCache),
            EmailAddress = contact.EMailAddress1,
            Induction = induction,
            DqtInduction = dqtInduction,
            InitialTeacherTraining = command.Include.HasFlag(GetPersonCommandIncludes.InitialTeacherTraining) ?
                Option.Some(ApplyRoleBasedResponseFilters(MapInitialTeacherTraining((await getIttTask!)), command.ApplyAppropriateBodyUserRestrictions)) :
                default,
            MandatoryQualifications = command.Include.HasFlag(GetPersonCommandIncludes.MandatoryQualifications) ?
                Option.Some(MapMandatoryQualifications((await getMqsTask!))) :
                default,
            Sanctions = command.Include.HasFlag(GetPersonCommandIncludes.Sanctions) ?
                Option.Some((await getAlertsTask!)
                    .Where(a => Constants.LegacyExposableSanctionCodes.Contains(a.AlertType!.DqtSanctionCode) && a.IsOpen)
                    .Select(s => new SanctionInfo()
                    {
                        Code = s.AlertType!.DqtSanctionCode!,
                        StartDate = s.StartDate
                    })
                    .AsReadOnly()) :
                default,
            Alerts = command.Include.HasFlag(GetPersonCommandIncludes.Alerts) ?
                Option.Some((await getAlertsTask!)
                    .Where(a =>
                    {
                        // The Legacy behavior is to only return prohibition-type alerts
                        if (command.ApplyLegacyAlertsBehavior)
                        {
                            return Constants.LegacyProhibitionSanctionCodes.Contains(a.AlertType!.DqtSanctionCode);
                        }

                        return !a.AlertType!.InternalOnly;
                    })
                    .Select(a => new Alert()
                    {
                        AlertId = a.AlertId,
                        AlertType = new()
                        {
                            AlertTypeId = a.AlertType!.AlertTypeId,
                            AlertCategory = new()
                            {
                                AlertCategoryId = a.AlertType.AlertCategory!.AlertCategoryId,
                                Name = a.AlertType.AlertCategory.Name
                            },
                            Name = a.AlertType.Name,
                            DqtSanctionCode = a.AlertType.DqtSanctionCode!
                        },
                        Details = a.Details,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate
                    })
                    .AsReadOnly()) :
                default,
            PreviousNames = command.Include.HasFlag(GetPersonCommandIncludes.PreviousNames) ?
                Option.Some(previousNames.Select(n => n).AsReadOnly()) :
                default,
            AllowIdSignInWithProhibitions = allowIdSignInWithProhibitions
        };
    }

    private static QtlsStatus MapQtlsStatus(DateTime? qtlsDate, bool? qtlsDateHasBeenSet)
    {
        return (qtlsDate, qtlsDateHasBeenSet) switch
        {
            (not null, _) => QtlsStatus.Active,
            (null, true) => QtlsStatus.Expired,
            _ => QtlsStatus.None
        };
    }

    private static (GetPersonResultDqtInduction? DqtInduction, GetPersonResultInduction Induction) MapInduction(
        PostgresModels.Person person)
    {
        var status = person.InductionStatus;
        var dqtStatusName = status.ToDqtInductionStatus(out var dqtStatusDescription);
        DqtInductionStatus? dqtStatus = dqtStatusName is not null
            ? Enum.Parse<DqtInductionStatus>(dqtStatusName, ignoreCase: true)
            : null;
        var startDate = person.InductionStartDate;
        var completedDate = person.InductionCompletedDate;

        var canGenerateCertificate = status is InductionStatus.Passed && completedDate.HasValue;
        var certificateUrl = canGenerateCertificate ? "/v3/certificates/induction" : null;

        var dqtInduction = dqtStatus is not null
            ? new GetPersonResultDqtInduction()
            {
                StartDate = startDate,
                EndDate = completedDate,
                Status = dqtStatus.Value,
                StatusDescription = dqtStatusDescription,
                CertificateUrl = certificateUrl,
                Periods = []
            }
            : null;

        var inductionInfo = new GetPersonResultInduction()
        {
            Status = status,
            StartDate = startDate,
            CompletedDate = completedDate,
            CertificateUrl = certificateUrl
        };

        return (dqtInduction, inductionInfo);
    }

    private static IReadOnlyCollection<IttResult> ApplyRoleBasedResponseFilters(
        IEnumerable<GetPersonResultInitialTeacherTraining> itt,
        bool userIsAppropriateBody) =>
        (userIsAppropriateBody
            ? itt
                .Where(i => i.Provider is not null)
                .Select(i => IttResult.FromT1(new GetPersonResultInitialTeacherTrainingForAppropriateBody()
                {
                    Provider = new GetPersonResultInitialTeacherTrainingProvider() { Name = i.Provider!.Name, Ukprn = i.Provider.Ukprn }
                }))
            : itt.Select(IttResult.FromT0))
        .AsReadOnly();

    private static IEnumerable<GetPersonResultInitialTeacherTraining> MapInitialTeacherTraining(dfeta_initialteachertraining[] itt) =>
        itt
            .Select(i => new GetPersonResultInitialTeacherTraining()
            {
                Qualification = MapIttQualification(i),
                ProgrammeType = i.dfeta_ProgrammeType?.ConvertToEnumByValue<dfeta_ITTProgrammeType, IttProgrammeType>(),
                ProgrammeTypeDescription =
                    i.dfeta_ProgrammeType?.ConvertToEnumByValue<dfeta_ITTProgrammeType, IttProgrammeType>().GetDescription(),
                StartDate = i.dfeta_ProgrammeStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                EndDate = i.dfeta_ProgrammeEndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                Result = i.dfeta_Result?.ConvertFromITTResult(),
                AgeRange = MapAgeRange(i.dfeta_AgeRangeFrom, i.dfeta_AgeRangeTo),
                Provider = MapIttProvider(i),
                Subjects = MapSubjects(i)
            })
            .OrderByDescending(i => i.StartDate);

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

    private static IReadOnlyCollection<GetPersonResultMandatoryQualification> MapMandatoryQualifications(PostgresModels.MandatoryQualification[] qualifications) =>
        qualifications
            .Where(q => q.EndDate.HasValue && q.Specialism.HasValue)
            .Select(mq => new GetPersonResultMandatoryQualification()
            {
                Awarded = mq.EndDate!.Value,
                Specialism = mq.Specialism!.Value.GetTitle()
            })
            .ToArray();
}
