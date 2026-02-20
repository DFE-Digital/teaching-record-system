using Optional;

namespace TeachingRecordSystem.Core.Services.Alerts;

public record UpdateAlertOptions
{
    public required Guid AlertId { get; init; }
    public Option<string?> Details { get; init; }
    public Option<string?> ExternalLink { get; init; }
    public Option<DateOnly?> StartDate { get; init; }
    public Option<DateOnly?> EndDate { get; init; }
}
