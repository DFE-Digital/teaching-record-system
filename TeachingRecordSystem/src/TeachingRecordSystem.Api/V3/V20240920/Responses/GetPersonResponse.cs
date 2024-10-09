using AutoMapper.Configuration.Annotations;
using Optional;
using TeachingRecordSystem.Api.Infrastructure.Mapping;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240101.ApiModels;
using TeachingRecordSystem.Api.V3.V20240606.Responses;
using TeachingRecordSystem.Api.V3.V20240920.ApiModels;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

[AutoMap(typeof(GetPersonResult))]
[GenerateVersionedDto(typeof(V20240606.Responses.GetPersonResponse), excludeMembers: ["Alerts", "Sanctions", "Induction", "InitialTeacherTraining"])]
public partial record GetPersonResponse
{
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
    public required Option<GetPersonResponseInduction?> Induction { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>> InitialTeacherTraining { get; init; }
}

[AutoMap(typeof(GetPersonResultInduction))]
[GenerateVersionedDto(typeof(V20240606.Responses.GetPersonResponseInduction), excludeMembers: ["Periods"])]
public partial record GetPersonResponseInduction;

[AutoMap(typeof(GetPersonResultInitialTeacherTraining))]
public partial record GetPersonResponseInitialTeacherTraining
{
    public required GetPersonResponseInitialTeacherTrainingProvider? Provider { get; init; }

    [ValueConverter(typeof(WrapWithOptionValueConverter<GetPersonResultInitialTeacherTrainingQualification, GetPersonResponseInitialTeacherTrainingQualification>))]
    public required Option<GetPersonResponseInitialTeacherTrainingQualification?> Qualification { get; init; }

    [ValueConverter(typeof(WrapWithOptionValueConverter<DateOnly?>))]
    public required Option<DateOnly?> StartDate { get; init; }

    [ValueConverter(typeof(WrapWithOptionValueConverter<DateOnly?>))]
    public required Option<DateOnly?> EndDate { get; init; }

    [ValueConverter(typeof(WrapWithOptionValueConverter<Core.SharedModels.IttProgrammeType, IttProgrammeType>))]
    public required Option<IttProgrammeType?> ProgrammeType { get; init; }

    [ValueConverter(typeof(WrapWithOptionValueConverter<string?>))]
    public required Option<string?> ProgrammeTypeDescription { get; init; }

    [ValueConverter(typeof(WrapWithOptionValueConverter<Core.SharedModels.IttOutcome, IttOutcome>))]
    public required Option<IttOutcome?> Result { get; init; }

    [ValueConverter(typeof(WrapWithOptionValueConverter<GetPersonResultInitialTeacherTrainingAgeRange, GetPersonResponseInitialTeacherTrainingAgeRange>))]
    public required Option<GetPersonResponseInitialTeacherTrainingAgeRange?> AgeRange { get; init; }

    [ValueConverter(typeof(WrapWithOptionValueConverter<IReadOnlyCollection<GetPersonResultInitialTeacherTrainingSubject>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingSubject>>))]
    public required Option<IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingSubject>> Subjects { get; init; }
}
