using System.Text.Json.Serialization;
using Optional;
using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Api.V3.V20240101;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240416.Responses;

public record GetTeacherResponse
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Option<bool> PendingNameChange { get; init; }
    public required Option<bool> PendingDateOfBirthChange { get; init; }
    public required string? Email { get; init; }
    public required GetTeacherResponseQts? Qts { get; init; }
    public required GetTeacherResponseEyts? Eyts { get; init; }
    public required Option<GetTeacherResponseInduction?> Induction { get; init; }
    public required Option<IReadOnlyCollection<GetTeacherResponseInitialTeacherTraining>> InitialTeacherTraining { get; init; }
    public required Option<IReadOnlyCollection<GetTeacherResponseNpqQualification>> NpqQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetTeacherResponseMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetTeacherResponseHigherEducationQualification>> HigherEducationQualifications { get; init; }
    public required Option<IReadOnlyCollection<SanctionInfo>> Sanctions { get; init; }
    public required Option<IReadOnlyCollection<AlertInfo>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }

    public static GetTeacherResponse Create(GetPersonResult source) => new()
    {
        Trn = source.Trn,
        FirstName = source.FirstName,
        MiddleName = source.MiddleName,
        LastName = source.LastName,
        DateOfBirth = source.DateOfBirth,
        NationalInsuranceNumber = source.NationalInsuranceNumber,
        PendingNameChange = source.PendingNameChange,
        PendingDateOfBirthChange = source.PendingDateOfBirthChange,
        Email = source.EmailAddress,
        Qts = source.Qts is { } qts ? GetTeacherResponseQts.Create(qts) : null,
        Eyts = source.Eyts is { } eyts ? GetTeacherResponseEyts.Create(eyts) : null,
        Induction = source.DqtInduction.Map(d => d is { } dqtInduction ? GetTeacherResponseInduction.Create(dqtInduction) : null),
        InitialTeacherTraining = source.InitialTeacherTraining.Map(itt =>
            itt.AsT0.Select(i => GetTeacherResponseInitialTeacherTraining.Create(i)).AsReadOnly()),
        NpqQualifications = default,
        MandatoryQualifications = source.MandatoryQualifications.Map(mqs =>
            mqs.Select(mq => GetTeacherResponseMandatoryQualification.Create(mq)).AsReadOnly()),
        HigherEducationQualifications = default,
        Sanctions = source.Sanctions.Map(sanctions => sanctions.Select(s => SanctionInfo.Create(s)).AsReadOnly()),
        Alerts = source.Alerts.Map(alerts => alerts.Select(a => AlertInfo.Create(a)).AsReadOnly()),
        PreviousNames = source.PreviousNames.Map(names => names.Select(n => NameInfo.Create(n)).AsReadOnly()),
        AllowIdSignInWithProhibitions = source.AllowIdSignInWithProhibitions
    };
}

public record GetTeacherResponseQts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }

    public static GetTeacherResponseQts Create(Operations.Common.QtsInfo source) => new()
    {
        Awarded = source.HoldsFrom,
        CertificateUrl = source.CertificateUrl,
        StatusDescription = source.StatusDescription
    };
}

public record GetTeacherResponseEyts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }

    public static GetTeacherResponseEyts Create(Operations.Common.EytsInfo source) => new()
    {
        Awarded = source.HoldsFrom,
        CertificateUrl = source.CertificateUrl,
        StatusDescription = source.StatusDescription
    };
}

public record GetTeacherResponseInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required DqtInductionStatus? Status { get; init; }
    public required string? StatusDescription { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
    public required IReadOnlyCollection<GetTeacherResponseInductionPeriod> Periods { get; init; }

    public static GetTeacherResponseInduction Create(GetPersonResultDqtInduction source) => new()
    {
        StartDate = source.StartDate,
        EndDate = source.EndDate,
        Status = DqtInductionStatus.Create(source.Status),
        StatusDescription = source.StatusDescription,
        CertificateUrl = source.CertificateUrl,
        Periods = source.Periods.Select(p => GetTeacherResponseInductionPeriod.Create(p)).AsReadOnly()
    };
}

public record GetTeacherResponseInductionPeriod
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required int? Terms { get; init; }
    public required GetTeacherResponseInductionPeriodAppropriateBody? AppropriateBody { get; init; }

    public static GetTeacherResponseInductionPeriod Create(GetPersonResultDqtInductionPeriod source) => new()
    {
        StartDate = source.StartDate,
        EndDate = source.EndDate,
        Terms = source.Terms,
        AppropriateBody = source.AppropriateBody is { } appropriateBody
            ? GetTeacherResponseInductionPeriodAppropriateBody.Create(appropriateBody)
            : null
    };
}

public record GetTeacherResponseInductionPeriodAppropriateBody
{
    public required string Name { get; init; }

    public static GetTeacherResponseInductionPeriodAppropriateBody Create(GetPersonResultInductionPeriodAppropriateBody source) => new()
    {
        Name = source.Name
    };
}

public record GetTeacherResponseInitialTeacherTraining
{
    public required GetTeacherResponseInitialTeacherTrainingQualification? Qualification { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required IttProgrammeType? ProgrammeType { get; init; }
    public required string? ProgrammeTypeDescription { get; init; }
    public required IttOutcome? Result { get; init; }
    public required GetTeacherResponseInitialTeacherTrainingAgeRange? AgeRange { get; init; }
    public required GetTeacherResponseInitialTeacherTrainingProvider? Provider { get; init; }
    public required IReadOnlyCollection<GetTeacherResponseInitialTeacherTrainingSubject> Subjects { get; init; }

    public static GetTeacherResponseInitialTeacherTraining Create(GetPersonResultInitialTeacherTraining source) => new()
    {
        Qualification = source.Qualification is { } qualification
            ? GetTeacherResponseInitialTeacherTrainingQualification.Create(qualification)
            : null,
        StartDate = source.StartDate,
        EndDate = source.EndDate,
        ProgrammeType = null,
        ProgrammeTypeDescription = null,
        Result = null,
        AgeRange = source.AgeRange is { } ageRange
            ? GetTeacherResponseInitialTeacherTrainingAgeRange.Create(ageRange)
            : null,
        Provider = source.Provider is { } provider
            ? GetTeacherResponseInitialTeacherTrainingProvider.Create(provider)
            : null,
        Subjects = source.Subjects.Select(s => GetTeacherResponseInitialTeacherTrainingSubject.Create(s)).AsReadOnly()
    };
}

public record GetTeacherResponseInitialTeacherTrainingQualification
{
    public required string Name { get; init; }

    public static GetTeacherResponseInitialTeacherTrainingQualification Create(GetPersonResultInitialTeacherTrainingQualification source) => new()
    {
        Name = source.Name
    };
}

public record GetTeacherResponseInitialTeacherTrainingAgeRange
{
    public required string Description { get; init; }

    public static GetTeacherResponseInitialTeacherTrainingAgeRange Create(GetPersonResultInitialTeacherTrainingAgeRange source) => new()
    {
        Description = source.Description
    };
}

public record GetTeacherResponseInitialTeacherTrainingProvider
{
    public required string Name { get; init; }
    public required string Ukprn { get; init; }

    public static GetTeacherResponseInitialTeacherTrainingProvider Create(GetPersonResultInitialTeacherTrainingProvider source) => new()
    {
        Name = source.Name,
        Ukprn = source.Ukprn
    };
}

public record GetTeacherResponseInitialTeacherTrainingSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }

    public static GetTeacherResponseInitialTeacherTrainingSubject Create(GetPersonResultInitialTeacherTrainingSubject source) => new()
    {
        Code = source.Code,
        Name = source.Name
    };
}

public record GetTeacherResponseNpqQualification
{
    public required DateOnly Awarded { get; init; }
    public required GetTeacherResponseNpqQualificationType Type { get; init; }
    public required string CertificateUrl { get; init; }
}

public record GetTeacherResponseNpqQualificationType
{
    public required NpqQualificationType Code { get; init; }
    public required string Name { get; init; }
}

public record GetTeacherResponseMandatoryQualification
{
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }

    public static GetTeacherResponseMandatoryQualification Create(GetPersonResultMandatoryQualification source) => new()
    {
        Awarded = source.EndDate,
        Specialism = source.Specialism
    };
}

public record GetTeacherResponseHigherEducationQualification
{
    public required string? Name { get; init; }
    public required DateOnly? Awarded { get; init; }
    public required IReadOnlyCollection<GetTeacherResponseHigherEducationQualificationSubject> Subjects { get; init; }
}

public record GetTeacherResponseHigherEducationQualificationSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}
