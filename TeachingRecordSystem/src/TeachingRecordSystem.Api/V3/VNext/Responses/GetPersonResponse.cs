using Optional;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.VNext.ApiModels;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

[AutoMap(typeof(GetPersonResult))]
[GenerateVersionedDto(typeof(V20240606.Responses.GetPersonResponse), excludeMembers: ["Alerts", "Sanctions"])]
public partial record GetPersonResponse
{
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
}
