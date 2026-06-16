using TeachingRecordSystem.Api.V3.Operations;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

[AutoMap(typeof(CreateNameChangeRequestResult))]
public record CreateNameChangeResponse
{
    public required string CaseNumber { get; init; }
}
