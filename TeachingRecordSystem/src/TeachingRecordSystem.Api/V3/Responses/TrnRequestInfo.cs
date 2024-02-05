using TeachingRecordSystem.Api.V3.Requests;

namespace TeachingRecordSystem.Api.V3.Responses;

public record TrnRequestInfo
{
    public required string RequestId { get; init; }
    public required TrnRequestPerson Person { get; init; }
    public required TrnRequestStatus Status { get; init; }
    public string? Trn { get; init; }
}
