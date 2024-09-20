using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240920.ApiModels;
using TeachingRecordSystem.Api.V3.V20240920.Requests;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

[GenerateVersionedDto(typeof(V20240814.Responses.FindPersonResponse), excludeMembers: ["Query", "Results"])]
public partial record FindPersonResponse
{
    public required FindPersonRequest Query { get; init; }
    public required IReadOnlyCollection<FindPersonResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonByLastNameAndDateOfBirthResultItem))]
[GenerateVersionedDto(typeof(V20240814.Responses.FindPersonResponseResult), excludeMembers: ["Sanctions"])]
public partial record FindPersonResponseResult
{
    public required IReadOnlyCollection<Alert> Alerts { get; init; }
}
