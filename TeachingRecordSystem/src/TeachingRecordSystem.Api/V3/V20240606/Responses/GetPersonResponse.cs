using Optional;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240101.ApiModels;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

[AutoMap(typeof(GetPersonResult))]
public partial record GetPersonResponse
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
    public required GetPersonResponseQts? Qts { get; init; }
    public required GetPersonResponseEyts? Eyts { get; init; }
    public required Option<GetPersonResponseInduction?> Induction { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>> InitialTeacherTraining { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseNpqQualification>> NpqQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseHigherEducationQualification>> HigherEducationQualifications { get; init; }
    public required Option<IReadOnlyCollection<SanctionInfo>> Sanctions { get; init; }
    public required Option<IReadOnlyCollection<AlertInfo>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }
}

[AutoMap(typeof(GetPersonResultQts))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseQts))]
public partial record GetPersonResponseQts;

[AutoMap(typeof(GetPersonResultEyts))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseEyts))]
public partial record GetPersonResponseEyts;

[AutoMap(typeof(GetPersonResultInduction))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInduction))]
public partial record GetPersonResponseInduction;

[AutoMap(typeof(GetPersonResultInductionPeriod))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInductionPeriod))]
public partial record GetPersonResponseInductionPeriod;

[AutoMap(typeof(GetPersonResultInductionPeriodAppropriateBody))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInductionPeriodAppropriateBody))]
public partial record GetPersonResponseInductionPeriodAppropriateBody;

[AutoMap(typeof(GetPersonResultInitialTeacherTraining))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInitialTeacherTraining))]
public partial record GetPersonResponseInitialTeacherTraining;

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingQualification))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInitialTeacherTrainingQualification))]
public partial record GetPersonResponseInitialTeacherTrainingQualification;

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingAgeRange))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInitialTeacherTrainingAgeRange))]
public partial record GetPersonResponseInitialTeacherTrainingAgeRange;

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingProvider))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInitialTeacherTrainingProvider))]
public partial record GetPersonResponseInitialTeacherTrainingProvider;

[AutoMap(typeof(GetPersonResultInitialTeacherTrainingSubject))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInitialTeacherTrainingSubject))]
public partial record GetPersonResponseInitialTeacherTrainingSubject;

[AutoMap(typeof(GetPersonResultNpqQualification))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseNpqQualification))]
public partial record GetPersonResponseNpqQualification;

[AutoMap(typeof(GetPersonResultNpqQualificationType))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseNpqQualificationType))]
public partial record GetPersonResponseNpqQualificationType;

[AutoMap(typeof(GetPersonResultMandatoryQualification))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseMandatoryQualification))]
public partial record GetPersonResponseMandatoryQualification;

[AutoMap(typeof(GetPersonResultHigherEducationQualification))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseHigherEducationQualification))]
public partial record GetPersonResponseHigherEducationQualification;

[AutoMap(typeof(GetPersonResultHigherEducationQualificationSubject))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseHigherEducationQualificationSubject))]
public partial record GetPersonResponseHigherEducationQualificationSubject;
