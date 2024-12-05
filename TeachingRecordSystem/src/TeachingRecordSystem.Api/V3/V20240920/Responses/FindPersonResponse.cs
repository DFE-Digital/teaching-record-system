using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240920.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

[GenerateVersionedDto(typeof(V20240814.Responses.FindPersonResponse), excludeMembers: ["Query", "Results"])]
public partial record FindPersonResponse
{
    public required FindPersonRequest Query { get; init; }
    public required IReadOnlyCollection<FindPersonResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonsResultItem))]
[GenerateVersionedDto(typeof(V20240814.Responses.FindPersonResponseResult), excludeMembers: ["Sanctions"])]
public partial record FindPersonResponseResult
{
    public required IReadOnlyCollection<Alert> Alerts { get; init; }
}
