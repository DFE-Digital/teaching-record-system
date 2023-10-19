namespace TeachingRecordSystem.Api.V3.ApiModels;

public record AlertInfo
{
    public required AlertType AlertType { get; init; }
    public required string DqtSanctionCode { get; init; }
}
