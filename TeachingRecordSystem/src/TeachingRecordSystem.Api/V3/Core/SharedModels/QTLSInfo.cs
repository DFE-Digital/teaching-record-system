namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record QTLSInfo
{
    public required DateOnly? AwardedDate { get; init; }
    public required string Trn { get; init; }
}
