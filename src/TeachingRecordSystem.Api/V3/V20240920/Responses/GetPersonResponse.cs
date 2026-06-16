using System.Text.Json.Serialization;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Api.V3.V20240101;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

public record GetPersonResponse
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Option<bool> PendingNameChange { get; init; }
    public required Option<bool> PendingDateOfBirthChange { get; init; }
    public required string? EmailAddress { get; init; }
    public required GetPersonResponseQts? Qts { get; init; }
    public required GetPersonResponseEyts? Eyts { get; init; }
    public required Option<GetPersonResponseInduction?> Induction { get; init; }
    public required Option<OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>>> InitialTeacherTraining { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseNpqQualification>> NpqQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseHigherEducationQualification>> HigherEducationQualifications { get; init; }
    public required Option<IReadOnlyCollection<SanctionInfo>> Sanctions { get; init; }
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }

    public static GetPersonResponse Create(GetPersonResult source) => new()
    {
        Trn = source.Trn,
        FirstName = source.FirstName,
        MiddleName = source.MiddleName,
        LastName = source.LastName,
        DateOfBirth = source.DateOfBirth,
        NationalInsuranceNumber = source.NationalInsuranceNumber,
        PendingNameChange = source.PendingNameChange,
        PendingDateOfBirthChange = source.PendingDateOfBirthChange,
        EmailAddress = source.EmailAddress,
        Qts = source.Qts is { } qts ? GetPersonResponseQts.Create(qts) : null,
        Eyts = source.Eyts is { } eyts ? GetPersonResponseEyts.Create(eyts) : null,
        Induction = source.DqtInduction.Map(d => d is { } dqtInduction ? GetPersonResponseInduction.Create(dqtInduction) : null),
        InitialTeacherTraining = source.InitialTeacherTraining.Map(itt => itt.Match(
            training => OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>>.FromT0(
                training.Select(i => GetPersonResponseInitialTeacherTraining.Create(i)).AsReadOnly()),
            forAppropriateBody => OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>>.FromT1(
                forAppropriateBody.Select(i => GetPersonResponseInitialTeacherTrainingForAppropriateBody.Create(i)).AsReadOnly()))),
        NpqQualifications = default,
        MandatoryQualifications = source.MandatoryQualifications.Map(mqs =>
            mqs.Select(mq => GetPersonResponseMandatoryQualification.Create(mq)).AsReadOnly()),
        HigherEducationQualifications = default,
        Sanctions = source.Sanctions.Map(sanctions => sanctions.Select(s => SanctionInfo.Create(s)).AsReadOnly()),
        Alerts = source.Alerts.Map(alerts => alerts.Select(a => Alert.Create(a)).AsReadOnly()),
        PreviousNames = source.PreviousNames.Map(names => names.Select(n => NameInfo.Create(n)).AsReadOnly()),
        AllowIdSignInWithProhibitions = source.AllowIdSignInWithProhibitions
    };
}

public record GetPersonResponseQts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }

    public static GetPersonResponseQts Create(Operations.Common.QtsInfo source) => new()
    {
        Awarded = source.HoldsFrom,
        CertificateUrl = source.CertificateUrl,
        StatusDescription = source.StatusDescription
    };
}

public record GetPersonResponseEyts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }

    public static GetPersonResponseEyts Create(Operations.Common.EytsInfo source) => new()
    {
        Awarded = source.HoldsFrom,
        CertificateUrl = source.CertificateUrl,
        StatusDescription = source.StatusDescription
    };
}

public record GetPersonResponseInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required DqtInductionStatus? Status { get; init; }
    public required string? StatusDescription { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }

    public static GetPersonResponseInduction Create(GetPersonResultDqtInduction source) => new()
    {
        StartDate = source.StartDate,
        EndDate = source.EndDate,
        Status = DqtInductionStatus.Create(source.Status),
        StatusDescription = source.StatusDescription,
        CertificateUrl = source.CertificateUrl
    };
}

public record GetPersonResponseInitialTeacherTraining
{
    public required GetPersonResponseInitialTeacherTrainingProvider? Provider { get; init; }
    public required GetPersonResponseInitialTeacherTrainingQualification? Qualification { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required IttProgrammeType? ProgrammeType { get; init; }
    public required string? ProgrammeTypeDescription { get; init; }
    public required IttOutcome? Result { get; init; }
    public required GetPersonResponseInitialTeacherTrainingAgeRange? AgeRange { get; init; }
    public required IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingSubject> Subjects { get; init; }

    public static GetPersonResponseInitialTeacherTraining Create(GetPersonResultInitialTeacherTraining source) => new()
    {
        Provider = source.Provider is { } provider
            ? GetPersonResponseInitialTeacherTrainingProvider.Create(provider)
            : null,
        Qualification = source.Qualification is { } qualification
            ? GetPersonResponseInitialTeacherTrainingQualification.Create(qualification)
            : null,
        StartDate = source.StartDate,
        EndDate = source.EndDate,
        ProgrammeType = null,
        ProgrammeTypeDescription = null,
        Result = null,
        AgeRange = source.AgeRange is { } ageRange
            ? GetPersonResponseInitialTeacherTrainingAgeRange.Create(ageRange)
            : null,
        Subjects = source.Subjects.Select(s => GetPersonResponseInitialTeacherTrainingSubject.Create(s)).AsReadOnly()
    };
}

public record GetPersonResponseInitialTeacherTrainingForAppropriateBody
{
    public required GetPersonResponseInitialTeacherTrainingProvider Provider { get; init; }

    public static GetPersonResponseInitialTeacherTrainingForAppropriateBody Create(GetPersonResultInitialTeacherTrainingForAppropriateBody source) => new()
    {
        Provider = GetPersonResponseInitialTeacherTrainingProvider.Create(source.Provider)
    };
}

public record GetPersonResponseInitialTeacherTrainingQualification
{
    public required string Name { get; init; }

    public static GetPersonResponseInitialTeacherTrainingQualification Create(GetPersonResultInitialTeacherTrainingQualification source) => new()
    {
        Name = source.Name
    };
}

public record GetPersonResponseInitialTeacherTrainingAgeRange
{
    public required string Description { get; init; }

    public static GetPersonResponseInitialTeacherTrainingAgeRange Create(GetPersonResultInitialTeacherTrainingAgeRange source) => new()
    {
        Description = source.Description
    };
}

public record GetPersonResponseInitialTeacherTrainingProvider
{
    public required string Name { get; init; }
    public required string Ukprn { get; init; }

    public static GetPersonResponseInitialTeacherTrainingProvider Create(GetPersonResultInitialTeacherTrainingProvider source) => new()
    {
        Name = source.Name,
        Ukprn = source.Ukprn
    };
}

public record GetPersonResponseInitialTeacherTrainingSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }

    public static GetPersonResponseInitialTeacherTrainingSubject Create(GetPersonResultInitialTeacherTrainingSubject source) => new()
    {
        Code = source.Code,
        Name = source.Name
    };
}

public record GetPersonResponseNpqQualification
{
    public required DateOnly Awarded { get; init; }
    public required GetPersonResponseNpqQualificationType Type { get; init; }
    public required string CertificateUrl { get; init; }
}

public record GetPersonResponseNpqQualificationType
{
    public required NpqQualificationType Code { get; init; }
    public required string Name { get; init; }
}

public record GetPersonResponseMandatoryQualification
{
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }

    public static GetPersonResponseMandatoryQualification Create(GetPersonResultMandatoryQualification source) => new()
    {
        Awarded = source.EndDate,
        Specialism = source.Specialism
    };
}

public record GetPersonResponseHigherEducationQualification
{
    public required string? Name { get; init; }
    public required DateOnly? Awarded { get; init; }
    public required IReadOnlyCollection<GetPersonResponseHigherEducationQualificationSubject> Subjects { get; init; }
}

public record GetPersonResponseHigherEducationQualificationSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}
