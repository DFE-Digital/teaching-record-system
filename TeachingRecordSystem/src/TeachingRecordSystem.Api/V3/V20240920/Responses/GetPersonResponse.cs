using Optional;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240920.ApiModels;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

[AutoMap(typeof(GetPersonResult))]
[GenerateVersionedDto(typeof(V20240606.Responses.GetPersonResponse), excludeMembers: ["Alerts", "Sanctions", "Induction"])]
public partial record GetPersonResponse
{
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
    public required Option<GetPersonResponseInduction?> Induction { get; init; }
}

[AutoMap(typeof(GetPersonResultInduction))]
[GenerateVersionedDto(typeof(V20240606.Responses.GetPersonResponseInduction), excludeMembers: ["Periods"])]
public partial record GetPersonResponseInduction;
