using TeachingRecordSystem.Api.V3.Implementation.Operations;
using InductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.InductionStatus;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

[GenerateVersionedDto(typeof(V20240920.Responses.FindPersonResponse), excludeMembers: ["Results"])]
public partial record FindPersonResponse
{
    public required IReadOnlyCollection<FindPersonResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonsResultItem))]
[GenerateVersionedDto(typeof(V20240920.Responses.FindPersonResponseResult), excludeMembers: ["InductionStatus"])]
public partial record FindPersonResponseResult
{
    public required InductionStatus InductionStatus { get; init; }
}
