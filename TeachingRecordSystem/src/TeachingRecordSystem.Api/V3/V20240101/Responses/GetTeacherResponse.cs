using System.Text.Json.Serialization;
using AutoMapper.Configuration.Annotations;
using Optional;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240101.ApiModels;

namespace TeachingRecordSystem.Api.V3.V20240101.Responses;

[AutoMap(typeof(GetPersonResult))]
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
    [SourceMember("EmailAddress")]
    public required string? Email { get; set; }
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
}

[AutoMap(typeof(Core.SharedModels.QtsInfo))]
public record GetTeacherResponseQts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

[AutoMap(typeof(Core.SharedModels.EytsInfo))]
public record GetTeacherResponseEyts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

[AutoMap(typeof(GetPersonResultInduction))]
public record GetTeacherResponseInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required InductionStatus? Status { get; init; }
    public required string? StatusDescription { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
    public required IReadOnlyCollection<GetTeacherResponseInductionPeriod> Periods { get; init; }
}

[AutoMap(typeof(GetPersonResultInductionPeriod))]
public record GetTeacherResponseInductionPeriod
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required int? Terms { get; init; }
    public required GetTeacherResponseInductionPeriodAppropriateBody? AppropriateBody { get; init; }
}

[AutoMap(typeof(GetPersonResultInductionPeriodAppropriateBody))]
public record GetTeacherResponseInductionPeriodAppropriateBody
{
    public required string Name { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTraining))]
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
}

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingQualification))]
public record GetTeacherResponseInitialTeacherTrainingQualification
{
    public required string Name { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingAgeRange))]
public record GetTeacherResponseInitialTeacherTrainingAgeRange
{
    public required string Description { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingProvider))]
public record GetTeacherResponseInitialTeacherTrainingProvider
{
    public required string Name { get; init; }
    public required string Ukprn { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingSubject))]
public record GetTeacherResponseInitialTeacherTrainingSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

[AutoMap(typeof(GetPersonResultNpqQualification))]
public record GetTeacherResponseNpqQualification
{
    public required DateOnly Awarded { get; init; }
    public required GetTeacherResponseNpqQualificationType Type { get; init; }
    public required string CertificateUrl { get; init; }
}

[AutoMap(typeof(GetPersonResultNpqQualificationType))]
public record GetTeacherResponseNpqQualificationType
{
    public required NpqQualificationType Code { get; init; }
    public required string Name { get; init; }
}

[AutoMap(typeof(GetPersonResultMandatoryQualification))]
public record GetTeacherResponseMandatoryQualification
{
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }
}

[AutoMap(typeof(GetPersonResultHigherEducationQualification))]
public record GetTeacherResponseHigherEducationQualification
{
    public required string? Name { get; init; }
    public required DateOnly? Awarded { get; init; }
    public required IReadOnlyCollection<GetTeacherResponseHigherEducationQualificationSubject> Subjects { get; init; }
}

[AutoMap(typeof(GetPersonResultHigherEducationQualificationSubject))]
public record GetTeacherResponseHigherEducationQualificationSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}
