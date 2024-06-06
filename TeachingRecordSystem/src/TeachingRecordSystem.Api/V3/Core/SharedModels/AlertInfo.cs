namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record AlertInfo
{
    public required AlertType AlertType { get; init; }
    public required string DqtSanctionCode { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
