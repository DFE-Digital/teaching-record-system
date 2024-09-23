namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record Alert
{
    public required Guid AlertId { get; init; }
    public required AlertType AlertType { get; init; }
    public required string? Details { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
