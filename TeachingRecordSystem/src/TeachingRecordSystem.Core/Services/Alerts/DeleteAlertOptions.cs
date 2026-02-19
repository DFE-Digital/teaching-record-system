namespace TeachingRecordSystem.Core.Services.Alerts;

public record DeleteAlertOptions
{
    public required Guid AlertId { get; init; }
}
