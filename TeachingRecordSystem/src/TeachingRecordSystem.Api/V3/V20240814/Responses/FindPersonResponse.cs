using AutoMapper.Configuration.Annotations;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240814.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240814.Responses;

[GenerateVersionedDto(typeof(V20240101.Responses.FindTeachersResponse), excludeMembers: ["Query", "Results"])]
public partial record FindPersonResponse
{
    public required FindPersonRequest Query { get; init; }
    public required IReadOnlyCollection<FindPersonResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonsResultItem))]
[GenerateVersionedDto(typeof(V20240101.Responses.FindTeachersResponseResult))]
public partial record FindPersonResponseResult
{
    [SourceMember(nameof(FindPersonsResultItem.DqtInductionStatus))]
    public required DqtInductionStatusInfo InductionStatus { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
}
