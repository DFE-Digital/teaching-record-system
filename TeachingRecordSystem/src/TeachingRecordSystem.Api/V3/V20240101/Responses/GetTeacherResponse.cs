using System.Text.Json.Serialization;
using AutoMapper.Configuration.Annotations;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

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
    [SourceMember(nameof(GetPersonResult.DqtInduction))]
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

[AutoMap(typeof(Implementation.Dtos.QtsInfo))]
public record GetTeacherResponseQts
{
    [SourceMember(nameof(Implementation.Dtos.QtsInfo.HoldsFrom))]
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

[AutoMap(typeof(Implementation.Dtos.EytsInfo))]
public record GetTeacherResponseEyts
{
    [SourceMember(nameof(Implementation.Dtos.QtsInfo.HoldsFrom))]
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

[AutoMap(typeof(GetPersonResultDqtInduction))]
public record GetTeacherResponseInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus? Status { get; init; }
    public required string? StatusDescription { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
    public required IReadOnlyCollection<GetTeacherResponseInductionPeriod> Periods { get; init; }
}

[AutoMap(typeof(GetPersonResultDqtInductionPeriod))]
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

[AutoMap(typeof(GetPersonResultMandatoryQualification))]
public record GetTeacherResponseMandatoryQualification
{
    [SourceMember("EndDate")]
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }
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
