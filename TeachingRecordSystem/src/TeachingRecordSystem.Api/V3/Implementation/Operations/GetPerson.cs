using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using EytsInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.EytsInfo;
using InductionInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.InductionInfo;
using QtlsStatus = TeachingRecordSystem.Core.Models.QtlsStatus;
using QtsInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.QtsInfo;
using RouteToProfessionalStatusStatus = TeachingRecordSystem.Core.Models.RouteToProfessionalStatusStatus;
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
    public required Guid MandatoryQualificationId { get; init; }
    public required DateOnly EndDate { get; init; }
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
    public required IReadOnlyCollection<PostgresModels.TrainingSubject> TrainingSubjects { get; init; }
    public required TrainingAgeSpecialism? TrainingAgeSpecialism { get; init; }
    public required TrainingCountry? TrainingCountry { get; init; }
    public required PostgresModels.TrainingProvider? TrainingProvider { get; init; }
    public required PostgresModels.DegreeType? DegreeType { get; init; }
    public required GetPersonResultRouteToProfessionalStatusInductionExemption InductionExemption { get; init; }
}

public record GetPersonResultRouteToProfessionalStatusForAppropriateBody
{
    public required PostgresModels.TrainingProvider TrainingProvider { get; init; }
}

public record GetPersonResultRouteToProfessionalStatusInductionExemption
{
    public required bool IsExempt { get; init; }
    public required IReadOnlyCollection<PostgresModels.InductionExemptionReason> ExemptionReasons { get; init; }
}

public class GetPersonHandler(TrsDbContext dbContext, ReferenceDataCache referenceDataCache)
{
    public async Task<ApiResult<GetPersonResult>> HandleAsync(GetPersonCommand command)
    {
        var options = command.Options ?? new GetPersonCommandOptions();

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

        var personQuery = dbContext.Persons
            .Where(p => p.Trn == command.Trn)
            .Include(p => p.Qualifications).AsSplitQuery()
            .Include(p => p.PreviousNames).AsSplitQuery();

        async Task<(bool PendingNameRequest, bool PendingDateOfBirthRequest)> GetPendingDetailChangesAsync()
        {
            var openTaskTypes = await dbContext.SupportTasks
                .Where(t => t.Person!.Trn == command.Trn &&
                    t.Status == SupportTaskStatus.Open &&
                    (t.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest || t.SupportTaskType == SupportTaskType.ChangeNameRequest))
                .Select(t => t.SupportTaskType)
                .Distinct()
                .ToArrayAsync();

            return (openTaskTypes.Any(t => t == SupportTaskType.ChangeNameRequest),
                    openTaskTypes.Any(t => t == SupportTaskType.ChangeDateOfBirthRequest));
        }

        (bool PendingNameRequest, bool PendingDateOfBirthRequest)? pendingDetailChanges = command.Include.HasFlag(GetPersonCommandIncludes.PendingDetailChanges) ?
            (await GetPendingDetailChangesAsync()) :
            null;

        if (command.Include.HasFlag(GetPersonCommandIncludes.Sanctions) || command.Include.HasFlag(GetPersonCommandIncludes.Alerts))
        {
            personQuery = personQuery.Include(p => p.Alerts).AsSplitQuery();
        }

        var person = await personQuery.SingleOrDefaultAsync();

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        // If a DateOfBirth or NationalInsuranceNumber was provided, ensure the record we've retrieved with the TRN matches
        if (command.DateOfBirth is DateOnly dateOfBirth && person.DateOfBirth != dateOfBirth)
        {
            return ApiError.PersonNotFound(command.Trn, dateOfBirth: dateOfBirth);
        }
        if (command.NationalInsuranceNumber is not null)
        {
            // Check the NINO on the Person first. If that fails, check workforce data (which may have different NINO(s) for the person).

            var normalizedNino = NationalInsuranceNumber.Normalize(command.NationalInsuranceNumber);
            var personNino = person.NationalInsuranceNumber;

            if (string.IsNullOrEmpty(personNino) || NationalInsuranceNumber.Normalize(personNino) != normalizedNino)
            {
                var employmentNinos = await dbContext.TpsEmployments
                    .Where(e => e.PersonId == person.PersonId && e.NationalInsuranceNumber != null)
                    .Select(e => e.NationalInsuranceNumber)
                    .Distinct()
                    .ToArrayAsync();

                if (!employmentNinos.Any(n => NationalInsuranceNumber.Normalize(n) == normalizedNino))
                {
                    return ApiError.PersonNotFound(command.Trn, nationalInsuranceNumber: command.NationalInsuranceNumber);
                }
            }
        }

        var allowIdSignInWithProhibitions = command.Include.HasFlag(GetPersonCommandIncludes.AllowIdSignInWithProhibitions) ?
            Option.Some(person.DqtAllowTeacherIdentitySignInWithProhibitions) :
            default;

        Option<GetPersonResultDqtInduction?> dqtInduction = default;
        Option<GetPersonResultInduction> induction = default;
        if (command.Include.HasFlag(GetPersonCommandIncludes.Induction))
        {
            var mappedInduction = await MapInductionAsync(person);
            dqtInduction = Option.Some(mappedInduction.DqtInduction);
            induction = Option.Some(mappedInduction.Induction);
        }

        return new GetPersonResult()
        {
            Trn = person.Trn!,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth!.Value,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            PendingNameChange = command.Include.HasFlag(GetPersonCommandIncludes.PendingDetailChanges) ? Option.Some(pendingDetailChanges!.Value.PendingNameRequest) : default,
            PendingDateOfBirthChange = command.Include.HasFlag(GetPersonCommandIncludes.PendingDetailChanges) ? Option.Some(pendingDetailChanges!.Value.PendingDateOfBirthRequest) : default,
            Qts = QtsInfo.Create(person),
            QtlsStatus = person.QtlsStatus,
            Eyts = EytsInfo.Create(person),
            EmailAddress = person.EmailAddress,
            Induction = induction,
            DqtInduction = dqtInduction,
            InitialTeacherTraining = command.Include.HasFlag(GetPersonCommandIncludes.InitialTeacherTraining) ?
                Option.Some(
                    ApplyRoleBasedResponseFilters(
                        await MapInitialTeacherTrainingAsync(
                            person.Qualifications!
                                .OfType<PostgresModels.RouteToProfessionalStatus>()
                                .OrderBy(q => q.CreatedOn)),
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
                Option.Some(
                    person.PreviousNames!
                        .Select(n => new NameInfo
                        {
                            FirstName = n.FirstName,
                            MiddleName = n.MiddleName,
                            LastName = n.LastName
                        })
                        .AsReadOnly()) :
                default,
            AllowIdSignInWithProhibitions = allowIdSignInWithProhibitions
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
                    .ToArrayAsync(),
                TrainingAgeSpecialism = TrainingAgeSpecialismExtensions.FromRoute(r),
                TrainingCountry = TrainingCountry.FromModel(r.TrainingCountry),
                TrainingProvider = r.TrainingProvider,
                DegreeType = r.DegreeType,
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

    private static IReadOnlyCollection<GetPersonResultMandatoryQualification> MapMandatoryQualifications(
            IEnumerable<PostgresModels.MandatoryQualification> qualifications) =>
        qualifications
            .Where(q => q is { EndDate: not null, Specialism: not null })
            .Select(mq => new GetPersonResultMandatoryQualification()
            {
                MandatoryQualificationId = mq.QualificationId,
                EndDate = mq.EndDate!.Value,
                Specialism = mq.Specialism!.Value.GetTitle()
            })
            .ToArray();
}

file static class Extensions
{
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, bool condition, T item) where T : notnull =>
        condition ? source.Append(item) : source;
}
