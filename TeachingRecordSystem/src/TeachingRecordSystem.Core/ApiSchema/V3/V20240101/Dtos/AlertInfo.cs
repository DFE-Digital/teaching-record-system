namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

public record AlertInfo
{
    public required AlertType AlertType { get; init; }
    public required string DqtSanctionCode { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
