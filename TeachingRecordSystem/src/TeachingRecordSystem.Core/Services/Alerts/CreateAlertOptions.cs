namespace TeachingRecordSystem.Core.Services.Alerts;

public record CreateAlertOptions
{
    public required Guid AlertTypeId { get; init; }
    public required Guid PersonId { get; init; }
    public required string? Details { get; init; }
    public required string? ExternalLink { get; init; }
    public required DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Reason { get; init; }
    public string? ReasonDetails { get; init; }
    public EventModels.File? EvidenceFile { get; init; }
    public required EventModels.RaisedByUserInfo CreatedBy { get; init; }
}
