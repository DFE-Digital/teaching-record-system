namespace TeachingRecordSystem.Api.V3.V20240606.ApiModels;

[AutoMap(typeof(Core.SharedModels.AlertInfo))]
public record AlertInfo
{
    public required AlertType AlertType { get; init; }
    public required string DqtSanctionCode { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
