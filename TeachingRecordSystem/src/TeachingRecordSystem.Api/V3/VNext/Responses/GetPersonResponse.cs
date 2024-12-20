using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

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
    public required Option<GetPersonResponseInduction> Induction { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>> InitialTeacherTraining { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseNpqQualification>> NpqQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<SanctionInfo>> Sanctions { get; init; }
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }
}

[AutoMap(typeof(Implementation.Dtos.QtsInfo))]
public record GetPersonResponseQts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

[AutoMap(typeof(Implementation.Dtos.EytsInfo))]
public record GetPersonResponseEyts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

[AutoMap(typeof(GetPersonResultInitialTeacherTraining))]
public record GetPersonResponseInitialTeacherTraining
{
    public required GetPersonResponseInitialTeacherTrainingProvider? Provider { get; init; }
    public required Option<GetPersonResponseInitialTeacherTrainingQualification?> Qualification { get; init; }
    public required Option<DateOnly?> StartDate { get; init; }
    public required Option<DateOnly?> EndDate { get; init; }
    public required Option<IttProgrammeType?> ProgrammeType { get; init; }
    public required Option<string?> ProgrammeTypeDescription { get; init; }
    public required Option<IttOutcome?> Result { get; init; }
    public required Option<GetPersonResponseInitialTeacherTrainingAgeRange?> AgeRange { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingSubject>> Subjects { get; init; }
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

[AutoMap(typeof(GetPersonResultNpqQualification))]
public record GetPersonResponseNpqQualification
{
    public required DateOnly Awarded { get; init; }
    public required GetPersonResponseNpqQualificationType Type { get; init; }
    public required string CertificateUrl { get; init; }
}

[AutoMap(typeof(GetPersonResultNpqQualificationType))]
public record GetPersonResponseNpqQualificationType
{
    public required NpqQualificationType Code { get; init; }
    public required string Name { get; init; }
}

[AutoMap(typeof(GetPersonResultMandatoryQualification))]
public record GetPersonResponseMandatoryQualification
{
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }
}

[AutoMap(typeof(GetPersonResultInduction))]
public record GetPersonResponseInduction : InductionInfo
{
    public required string? CertificateUrl { get; init; }
}
