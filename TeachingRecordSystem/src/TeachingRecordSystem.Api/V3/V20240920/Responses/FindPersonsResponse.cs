using AutoMapper.Configuration.Annotations;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

[AutoMap(typeof(FindPersonsResult))]
[GenerateVersionedDto(typeof(V20240814.Responses.FindPersonsResponse), excludeMembers: ["Results"])]
public partial record FindPersonsResponse
{
    [SourceMember(nameof(FindPersonsResult.Items))]
    public required IReadOnlyCollection<FindPersonsResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonsResultItem))]
[GenerateVersionedDto(typeof(V20240814.Responses.FindPersonsResponseResult), excludeMembers: ["Sanctions"])]
public partial record FindPersonsResponseResult
{
    public required IReadOnlyCollection<Alert> Alerts { get; init; }
}
