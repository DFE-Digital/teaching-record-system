namespace TeachingRecordSystem.Api.V3.V20240101.ApiModels;

public record TrnRequestInfo
{
    public required string RequestId { get; init; }
    public required TrnRequestPerson Person { get; init; }
    public required TrnRequestStatus Status { get; init; }
    public required string? Trn { get; init; }
}
