using AutoMapper.Configuration.Annotations;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240606.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

[AutoMap(typeof(GetPersonResult))]
[GenerateVersionedDto(typeof(V20240606.Responses.GetPersonResponse), excludeMembers: ["Alerts", "Sanctions", "Induction", "InitialTeacherTraining"])]
public partial record GetPersonResponse
{
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
    [SourceMember(nameof(GetPersonResult.DqtInduction))]
    public required Option<GetPersonResponseInduction?> Induction { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>> InitialTeacherTraining { get; init; }
}

[AutoMap(typeof(GetPersonResultDqtInduction))]
[GenerateVersionedDto(typeof(V20240606.Responses.GetPersonResponseInduction), excludeMembers: ["Periods"])]
public partial record GetPersonResponseInduction;

[AutoMap(typeof(GetPersonResultInitialTeacherTraining))]
public partial record GetPersonResponseInitialTeacherTraining
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
