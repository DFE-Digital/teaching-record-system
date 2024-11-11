namespace TeachingRecordSystem.Api.V3.V20240912.ApiModels;

[AutoMap(typeof(Core.SharedModels.QtlsResult))]
public record QtlsResponse
{
    public required DateOnly? QtsDate { get; init; }
    public required string Trn { get; init; }
}
