namespace TeachingRecordSystem.Core.Services.Alerts;

public record DeleteAlertOptions
{
    public required Guid AlertId { get; init; }
    public string? ReasonDetails { get; init; }
    public EventModels.File? EvidenceFile { get; init; }
}
