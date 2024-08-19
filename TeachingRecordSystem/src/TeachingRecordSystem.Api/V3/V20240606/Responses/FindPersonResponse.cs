using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240606.Requests;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

[GenerateVersionedDto(typeof(V20240101.Responses.FindTeachersResponse), excludeMembers: ["Query", "Results"])]
public partial record FindPersonResponse
{
    public required FindPersonRequest Query { get; init; }
    public required IReadOnlyCollection<FindPersonResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonByLastNameAndDateOfBirthResultItem))]
[GenerateVersionedDto(typeof(V20240101.Responses.FindTeachersResponseResult))]
public partial record FindPersonResponseResult;
