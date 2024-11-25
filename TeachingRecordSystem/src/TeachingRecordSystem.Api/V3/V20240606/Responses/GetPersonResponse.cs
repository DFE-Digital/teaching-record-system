using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

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

[AutoMap(typeof(Implementation.Dtos.QtsInfo))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseQts))]
public partial record GetPersonResponseQts;

[AutoMap(typeof(Implementation.Dtos.EytsInfo))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseEyts))]
public partial record GetPersonResponseEyts;

[AutoMap(typeof(GetPersonResultInduction))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInduction), excludeMembers: ["Periods"])]
public partial record GetPersonResponseInduction
{
    public required IReadOnlyCollection<GetPersonResponseInductionPeriod> Periods { get; init; }
}

[AutoMap(typeof(GetPersonResultInductionPeriod))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInductionPeriod), excludeMembers: ["AppropriateBody"])]
public partial record GetPersonResponseInductionPeriod
{
    public required GetPersonResponseInductionPeriodAppropriateBody? AppropriateBody { get; init; }
}

[AutoMap(typeof(GetPersonResultInductionPeriodAppropriateBody))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInductionPeriodAppropriateBody))]
public partial record GetPersonResponseInductionPeriodAppropriateBody;

[AutoMap(typeof(GetPersonResultInitialTeacherTraining))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseInitialTeacherTraining), excludeMembers: ["Qualification", "AgeRange", "Provider", "Subjects"])]
public partial record GetPersonResponseInitialTeacherTraining
{
    public required GetPersonResponseInitialTeacherTrainingQualification? Qualification { get; init; }
    public required GetPersonResponseInitialTeacherTrainingAgeRange? AgeRange { get; init; }
    public required GetPersonResponseInitialTeacherTrainingProvider? Provider { get; init; }
    public required IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingSubject> Subjects { get; init; }
}

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
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseNpqQualification), excludeMembers: "Type")]
public partial record GetPersonResponseNpqQualification
{
    public required GetPersonResponseNpqQualificationType Type { get; init; }
}

[AutoMap(typeof(GetPersonResultNpqQualificationType))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseNpqQualificationType))]
public partial record GetPersonResponseNpqQualificationType;

[AutoMap(typeof(GetPersonResultMandatoryQualification))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseMandatoryQualification))]
public partial record GetPersonResponseMandatoryQualification;

[AutoMap(typeof(GetPersonResultHigherEducationQualification))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseHigherEducationQualification), excludeMembers: "Subjects")]
public partial record GetPersonResponseHigherEducationQualification
{
    public required IReadOnlyCollection<GetPersonResponseHigherEducationQualificationSubject> Subjects { get; init; }
}

[AutoMap(typeof(GetPersonResultHigherEducationQualificationSubject))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponseHigherEducationQualificationSubject))]
public partial record GetPersonResponseHigherEducationQualificationSubject;
