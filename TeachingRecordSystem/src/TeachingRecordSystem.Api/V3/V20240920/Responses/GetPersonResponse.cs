using System.Text.Json.Serialization;
using AutoMapper.Configuration.Annotations;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

[AutoMap(typeof(GetPersonResult))]
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
    [SourceMember(nameof(GetPersonResult.DqtInduction))]
    public required Option<GetPersonResponseInduction?> Induction { get; init; }
    public required Option<OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>>> InitialTeacherTraining { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseNpqQualification>> NpqQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseHigherEducationQualification>> HigherEducationQualifications { get; init; }
    public required Option<IReadOnlyCollection<SanctionInfo>> Sanctions { get; init; }
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }
}

[AutoMap(typeof(Implementation.Dtos.QtsInfo))]
public record GetPersonResponseQts
{
    [SourceMember(nameof(Implementation.Dtos.QtsInfo.HoldsFrom))]
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

[AutoMap(typeof(Implementation.Dtos.EytsInfo))]
public record GetPersonResponseEyts
{
    [SourceMember(nameof(Implementation.Dtos.QtsInfo.HoldsFrom))]
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

[AutoMap(typeof(GetPersonResultDqtInduction))]
public record GetPersonResponseInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required DqtInductionStatus? Status { get; init; }
    public required string? StatusDescription { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTraining))]
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
}

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingForAppropriateBody))]
public record GetPersonResponseInitialTeacherTrainingForAppropriateBody
{
    public required GetPersonResponseInitialTeacherTrainingProvider Provider { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingQualification))]
public record GetPersonResponseInitialTeacherTrainingQualification
{
    public required string Name { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingAgeRange))]
public record GetPersonResponseInitialTeacherTrainingAgeRange
{
    public required string Description { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingProvider))]
public record GetPersonResponseInitialTeacherTrainingProvider
{
    public required string Name { get; init; }
    public required string Ukprn { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingSubject))]
public record GetPersonResponseInitialTeacherTrainingSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
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

[AutoMap(typeof(GetPersonResultMandatoryQualification))]
public record GetPersonResponseMandatoryQualification
{
    [SourceMember("EndDate")]
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }
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
