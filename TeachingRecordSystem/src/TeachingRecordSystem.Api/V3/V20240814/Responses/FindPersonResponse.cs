using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240814.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;
using InductionStatusInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos.InductionStatusInfo;

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
    public required InductionStatusInfo InductionStatus { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
}
