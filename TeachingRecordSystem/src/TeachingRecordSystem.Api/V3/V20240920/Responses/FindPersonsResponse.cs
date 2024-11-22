using AutoMapper.Configuration.Annotations;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

[AutoMap(typeof(FindPersonsByTrnAndDateOfBirthResult))]
[GenerateVersionedDto(typeof(V20240814.Responses.FindPersonsResponse), excludeMembers: ["Results"])]
public partial record FindPersonsResponse
{
    [SourceMember(nameof(FindPersonsByTrnAndDateOfBirthResult.Items))]
    public required IReadOnlyCollection<FindPersonsResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonsByTrnAndDateOfBirthResultItem))]
[GenerateVersionedDto(typeof(V20240814.Responses.FindPersonsResponseResult), excludeMembers: ["Sanctions"])]
public partial record FindPersonsResponseResult
{
    public required IReadOnlyCollection<Alert> Alerts { get; init; }
}
