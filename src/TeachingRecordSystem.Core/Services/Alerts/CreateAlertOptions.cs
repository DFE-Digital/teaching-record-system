namespace TeachingRecordSystem.Core.Services.Alerts;

public record CreateAlertOptions
{
    public required Guid PersonId { get; init; }
    public required Guid AlertTypeId { get; init; }
    public required string? Details { get; init; }
    public required string? ExternalLink { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
