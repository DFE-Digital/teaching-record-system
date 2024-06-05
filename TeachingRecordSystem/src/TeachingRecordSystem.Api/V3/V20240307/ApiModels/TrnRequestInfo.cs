namespace TeachingRecordSystem.Api.V3.V20240307.ApiModels;

[AutoMap(typeof(Core.SharedModels.TrnRequestInfo))]
public record TrnRequestInfo
{
    public required string RequestId { get; init; }
    public required TrnRequestPerson Person { get; init; }
    public required TrnRequestStatus Status { get; init; }
    public required string? Trn { get; init; }
}
