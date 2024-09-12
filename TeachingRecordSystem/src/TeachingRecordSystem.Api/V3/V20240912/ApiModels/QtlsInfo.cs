namespace TeachingRecordSystem.Api.V3.V20240912.ApiModels;

[AutoMap(typeof(Core.SharedModels.QtlsInfo))]
public record QtlsInfo
{
    public required DateOnly? QtsDate { get; init; }
    public required string Trn { get; init; }
}
