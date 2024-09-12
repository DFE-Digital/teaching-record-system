using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.VNext.ApiModels;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

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
