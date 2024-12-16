using AutoMapper.Configuration.Annotations;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using InductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.InductionStatus;
using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.QtlsStatus;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

[AutoMap(typeof(FindPersonsResult))]
[GenerateVersionedDto(typeof(V20240920.Responses.FindPersonsResponse), excludeMembers: ["Results"])]
public partial record FindPersonsResponse
{
    [SourceMember(nameof(FindPersonsResult.Items))]
    public required IReadOnlyCollection<FindPersonsResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonsResultItem))]
[GenerateVersionedDto(typeof(V20240920.Responses.FindPersonsResponseResult), excludeMembers: ["InductionStatus", "QtlsStatus"])]
public partial record FindPersonsResponseResult
{
    public required InductionStatus InductionStatus { get; init; }
    public required QtlsStatus QtlsStatus { get; set; }
}
