using System.Text;
using Microsoft.Xrm.Sdk.Query;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using EytsInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.EytsInfo;
using InductionInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.InductionInfo;
using QtlsStatus = TeachingRecordSystem.Core.Models.QtlsStatus;
using QtsInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.QtsInfo;
using TrainingAgeSpecialism = TeachingRecordSystem.Api.V3.Implementation.Dtos.TrainingAgeSpecialism;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

using IttResult = OneOf<IReadOnlyCollection<GetPersonResultInitialTeacherTraining>, IReadOnlyCollection<GetPersonResultInitialTeacherTrainingForAppropriateBody>>;
using RoutesResult = OneOf<IReadOnlyCollection<GetPersonResultRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResultRouteToProfessionalStatusForAppropriateBody>>;

public record GetPersonCommand(
    string Trn,
    GetPersonCommandIncludes Include,
    DateOnly? DateOfBirth,
    string? NationalInsuranceNumber,
    GetPersonCommandOptions? Options = null);

public record GetPersonCommandOptions
{
    public bool ApplyLegacyAlertsBehavior { get; init; }
    public bool ApplyAppropriateBodyUserRestrictions { get; init; }
}

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
    RoutesToProfessionalStatuses = 1 << 10
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
    public required Option<IttResult> InitialTeacherTraining { get; init; }
    public required Option<RoutesResult> RoutesToProfessionalStatuses { get; init; }
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

public record GetPersonResultRouteToProfessionalStatus
{
    public required Guid RouteToProfessionalStatusId { get; init; }
    public required PostgresModels.RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
    public required RouteToProfessionalStatusStatus Status { get; init; }
    public required DateOnly? HoldsFrom { get; init; }
    public required DateOnly? TrainingStartDate { get; init; }
    public required DateOnly? TrainingEndDate { get; init; }
    public required IReadOnlyCollection<TrainingSubject> TrainingSubjects { get; init; }
    public required TrainingAgeSpecialism? TrainingAgeSpecialism { get; init; }
    public required TrainingCountry? TrainingCountry { get; init; }
    public required TrainingProvider? TrainingProvider { get; init; }
    public required DegreeType? DegreeType { get; init; }
    public required GetPersonResultRouteToProfessionalStatusInductionExemption InductionExemption { get; init; }
}

public record GetPersonResultRouteToProfessionalStatusForAppropriateBody
{
    public required TrainingProvider TrainingProvider { get; init; }
}

public record GetPersonResultRouteToProfessionalStatusInductionExemption
{
    public required bool IsExempt { get; init; }
    public required IReadOnlyCollection<PostgresModels.InductionExemptionReason> ExemptionReasons { get; init; }
}

public class GetPersonHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    IDataverseAdapter dataverseAdapter,
    PreviousNameHelper previousNameHelper,
    IFeatureProvider featureProvider)
{
    public async Task<ApiResult<GetPersonResult>> HandleAsync(GetPersonCommand command)
    {
        var options = command.Options ?? new GetPersonCommandOptions();
        var routesMigrated = featureProvider.IsEnabled(FeatureNames.RoutesToProfessionalStatus);

        if (options.ApplyAppropriateBodyUserRestrictions)
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
        // This lock should be used around all calls to DataverseAdapter.
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

        var contact = contactDetail.Contact;
        var personId = contact.Id;

        var personQuery = dbContext.Persons
            .Where(p => p.PersonId == personId)
            .Include(p => p.Qualifications).AsSplitQuery();

        var getIttTask = command.Include.HasFlag(GetPersonCommandIncludes.InitialTeacherTraining) && !routesMigrated ?
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

        if (command.Include.HasFlag(GetPersonCommandIncludes.Sanctions) || command.Include.HasFlag(GetPersonCommandIncludes.Alerts))
        {
            personQuery = personQuery.Include(p => p.Alerts).AsSplitQuery();
        }

        IEnumerable<NameInfo> previousNames = previousNameHelper.GetFullPreviousNames(contactDetail.PreviousNames, contactDetail.Contact)
            .Select(name => new NameInfo()
            {
                FirstName = name.FirstName,
                MiddleName = name.MiddleName,
                LastName = name.LastName
            })
            .AsReadOnly();

        var person = await personQuery.SingleAsync();

        var firstName = contact.ResolveFirstName();
        var middleName = contact.ResolveMiddleName();
        var lastName = contact.ResolveLastName();

        var qtsRegistrations = !routesMigrated
            ? (await crmQueryDispatcher.ExecuteQueryAsync(
                new GetActiveQtsRegistrationsByContactIdsQuery(
                    [contact.Id],
                    new ColumnSet(
                        dfeta_qtsregistration.Fields.dfeta_QTSDate,
                        dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                        dfeta_qtsregistration.Fields.dfeta_name,
                        dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                        dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                        dfeta_qtsregistration.Fields.dfeta_PersonId))))[contact.Id]
            : null;

        var qtsInfo = routesMigrated
            ? QtsInfo.Create(person)
            : await QtsInfo.CreateAsync(qtsRegistrations!, contact.dfeta_qtlsdate, referenceDataCache);

        var eytsInfo = routesMigrated
            ? EytsInfo.Create(person)
            : await EytsInfo.CreateAsync(qtsRegistrations!, referenceDataCache);

        var allowIdSignInWithProhibitions = command.Include.HasFlag(GetPersonCommandIncludes.AllowIdSignInWithProhibitions) ?
            Option.Some(contact.dfeta_AllowIDSignInWithProhibitions == true) :
            default;

        Option<GetPersonResultDqtInduction?> dqtInduction = default;
        Option<GetPersonResultInduction> induction = default;
        if (command.Include.HasFlag(GetPersonCommandIncludes.Induction))
        {
            var mappedInduction = await MapInductionAsync(person);
            dqtInduction = Option.Some(mappedInduction.DqtInduction);
            induction = Option.Some(mappedInduction.Induction);
        }

        var qtlsStatus = routesMigrated ? person.QtlsStatus : MapQtlsStatus_Dqt(contact.dfeta_qtlsdate, contact.dfeta_QtlsDateHasBeenSet);

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
            Qts = qtsInfo,
            QtlsStatus = qtlsStatus,
            Eyts = eytsInfo,
            EmailAddress = contact.EMailAddress1,
            Induction = induction,
            DqtInduction = dqtInduction,
            InitialTeacherTraining = command.Include.HasFlag(GetPersonCommandIncludes.InitialTeacherTraining) ?
                Option.Some(
                    ApplyRoleBasedResponseFilters(
                        routesMigrated
                            ? await MapInitialTeacherTrainingAsync(
                                person.Qualifications!
                                    .OfType<PostgresModels.RouteToProfessionalStatus>()
                                    .OrderBy(q => q.CreatedOn))
                            : MapInitialTeacherTrainingFromDqt((await getIttTask!)),
                        options.ApplyAppropriateBodyUserRestrictions)) :
                default,
            RoutesToProfessionalStatuses = command.Include.HasFlag(GetPersonCommandIncludes.RoutesToProfessionalStatuses) ?
                Option.Some(
                    ApplyRoleBasedResponseFilters(
                        await MapRoutesToProfessionalStatusesAsync(
                            person.Qualifications!
                                .OfType<PostgresModels.RouteToProfessionalStatus>()
                                .OrderBy(q => q.CreatedOn)),
                        options.ApplyAppropriateBodyUserRestrictions)) :
                default,
            MandatoryQualifications = command.Include.HasFlag(GetPersonCommandIncludes.MandatoryQualifications) ?
                Option.Some(MapMandatoryQualifications(
                    person.Qualifications!
                        .OfType<PostgresModels.MandatoryQualification>()
                        .OrderBy(q => q.CreatedOn))) :
                default,
            Sanctions = command.Include.HasFlag(GetPersonCommandIncludes.Sanctions) ?
                Option.Some(person.Alerts!
                    .Where(a => Constants.LegacyExposableSanctionCodes.Contains(a.AlertType!.DqtSanctionCode) && a.IsOpen)
                    .OrderBy(a => a.CreatedOn)
                    .Select(s => new SanctionInfo()
                    {
                        Code = s.AlertType!.DqtSanctionCode!,
                        StartDate = s.StartDate
                    })
                    .AsReadOnly()) :
                default,
            Alerts = command.Include.HasFlag(GetPersonCommandIncludes.Alerts) ?
                Option.Some(person.Alerts!
                    .Where(a =>
                    {
                        // The Legacy behavior is to only return prohibition-type alerts
                        if (options.ApplyLegacyAlertsBehavior)
                        {
                            return Constants.LegacyProhibitionSanctionCodes.Contains(a.AlertType!.DqtSanctionCode);
                        }

                        return !a.AlertType!.InternalOnly;
                    })
                    .OrderBy(a => a.CreatedOn)
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

    private static QtlsStatus MapQtlsStatus_Dqt(DateTime? qtlsDate, bool? qtlsDateHasBeenSet)
    {
        return (qtlsDate, qtlsDateHasBeenSet) switch
        {
            (not null, _) => QtlsStatus.Active,
            (null, true) => QtlsStatus.Expired,
            _ => QtlsStatus.None
        };
    }

    private async Task<(GetPersonResultDqtInduction? DqtInduction, GetPersonResultInduction Induction)> MapInductionAsync(PostgresModels.Person person)
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
            CertificateUrl = certificateUrl,
            ExemptionReasons = await person.GetAllInductionExemptionReasonIds()
                .ToAsyncEnumerable()
                .SelectAwait(async id => await referenceDataCache.GetInductionExemptionReasonByIdAsync(id))
                .ToArrayAsync(),
        };

        return (dqtInduction, inductionInfo);
    }

    private static IttResult ApplyRoleBasedResponseFilters(
        IEnumerable<GetPersonResultInitialTeacherTraining> itt,
        bool userIsAppropriateBody) =>
        userIsAppropriateBody
            ? IttResult.FromT1(itt
                .Where(i => i.Provider is not null)
                .Select(i => new GetPersonResultInitialTeacherTrainingForAppropriateBody()
                {
                    Provider = new GetPersonResultInitialTeacherTrainingProvider() { Name = i.Provider!.Name, Ukprn = i.Provider.Ukprn }
                })
                .AsReadOnly())
            : IttResult.FromT0(itt.AsReadOnly());

    private static RoutesResult ApplyRoleBasedResponseFilters(
        IEnumerable<GetPersonResultRouteToProfessionalStatus> routes,
        bool userIsAppropriateBody) =>
        userIsAppropriateBody
            ? RoutesResult.FromT1(routes
                .Where(i => i.TrainingProvider is not null)
                .Select(i => new GetPersonResultRouteToProfessionalStatusForAppropriateBody()
                {
                    TrainingProvider = i.TrainingProvider!
                })
                .AsReadOnly())
            : RoutesResult.FromT0(routes.AsReadOnly());

    private async Task<IEnumerable<GetPersonResultRouteToProfessionalStatus>> MapRoutesToProfessionalStatusesAsync(
        IEnumerable<PostgresModels.RouteToProfessionalStatus> routes) =>
        await routes
            .ToAsyncEnumerable()
            .SelectAwait(async r => new GetPersonResultRouteToProfessionalStatus()
            {
                RouteToProfessionalStatusId = r.QualificationId,
                RouteToProfessionalStatusType = r.RouteToProfessionalStatusType!,
                Status = r.Status,
                HoldsFrom = r.HoldsFrom,
                TrainingStartDate = r.TrainingStartDate,
                TrainingEndDate = r.TrainingEndDate,
                TrainingSubjects = await r.TrainingSubjectIds.ToAsyncEnumerable()
                    .SelectAwait(async id => await referenceDataCache.GetTrainingSubjectByIdAsync(id))
                    .Select(TrainingSubject.FromModel)
                    .ToArrayAsync(),
                TrainingAgeSpecialism = TrainingAgeSpecialismExtensions.FromRoute(r),
                TrainingCountry = TrainingCountry.FromModel(r.TrainingCountry),
                TrainingProvider = TrainingProvider.FromModel(r.TrainingProvider),
                DegreeType = DegreeType.FromModel(r.DegreeType),
                InductionExemption = new GetPersonResultRouteToProfessionalStatusInductionExemption()
                {
                    IsExempt = r.ExemptFromInduction == true || r.ExemptFromInductionDueToQtsDate == true,
                    ExemptionReasons = Array.Empty<PostgresModels.InductionExemptionReason>()
                        .AppendIf(
                            r.ExemptFromInduction == true,
                            r.RouteToProfessionalStatusType!.InductionExemptionReason!)
                        .AppendIf(
                            r.ExemptFromInductionDueToQtsDate == true,
                            await referenceDataCache.GetInductionExemptionReasonByIdAsync(PostgresModels.InductionExemptionReason.QualifiedBefore7May2000Id))
                        .AsReadOnly()
                }
            })
            .ToArrayAsync();

    private async Task<IEnumerable<GetPersonResultInitialTeacherTraining>> MapInitialTeacherTrainingAsync(
        IEnumerable<PostgresModels.RouteToProfessionalStatus> routes) =>
        await routes
            .ToAsyncEnumerable()
            .SelectAwait(async r => new GetPersonResultInitialTeacherTraining()
            {
                Qualification = null,
                StartDate = r.TrainingStartDate,
                EndDate = r.TrainingEndDate,
                ProgrammeType = null,
                ProgrammeTypeDescription = null,
                Result = null,
                AgeRange = MapAgeRange(r.TrainingAgeSpecialismType, r.TrainingAgeSpecialismRangeFrom, r.TrainingAgeSpecialismRangeTo),
                Provider = r.TrainingProvider is { Ukprn: not null } trainingProvider
                    ? new GetPersonResultInitialTeacherTrainingProvider { Name = trainingProvider.Name, Ukprn = trainingProvider.Ukprn }
                    : null,
                Subjects = await r.TrainingSubjectIds.ToAsyncEnumerable()
                    .SelectAwait(async id => await referenceDataCache.GetTrainingSubjectByIdAsync(id))
                    .Select(subject => new GetPersonResultInitialTeacherTrainingSubject() { Code = subject.Reference, Name = subject.Name })
                    .ToArrayAsync()
            })
            .ToArrayAsync();

    private static IEnumerable<GetPersonResultInitialTeacherTraining> MapInitialTeacherTrainingFromDqt(dfeta_initialteachertraining[] itt) =>
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

    private static GetPersonResultInitialTeacherTrainingAgeRange? MapAgeRange(
        TrainingAgeSpecialismType? trainingAge,
        int? from,
        int? to)
    {
        if (trainingAge is null)
        {
            return null;
        }

        return new GetPersonResultInitialTeacherTrainingAgeRange
        {
            Description = from is not null && to is not null ? $"{from} to {to}" :
                from is not null ? $"{from}" :
                to is not null ? $"{to}" :
                trainingAge.GetDisplayName()!
        };
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

    private static IReadOnlyCollection<GetPersonResultMandatoryQualification> MapMandatoryQualifications(
            IEnumerable<PostgresModels.MandatoryQualification> qualifications) =>
        qualifications
            .Where(q => q is { EndDate: not null, Specialism: not null })
            .Select(mq => new GetPersonResultMandatoryQualification()
            {
                Awarded = mq.EndDate!.Value,
                Specialism = mq.Specialism!.Value.GetTitle()
            })
            .ToArray();
}

file static class Extensions
{
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, bool condition, T item) where T : notnull =>
        condition ? source.Append(item) : source;
}
