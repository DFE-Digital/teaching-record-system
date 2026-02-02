namespace TeachingRecordSystem.Core.Events;

public class AlertUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
    public required EventModels.Alert Alert { get; init; }
    public required EventModels.Alert OldAlert { get; init; }
    public required string? Reason { get; init; }
    public required string? ReasonDetails { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
    public required AlertUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum AlertUpdatedEventChanges
{
    None = 0,
    Details = 1 << 0,
    ExternalLink = 1 << 2,
    StartDate = 1 << 3,
    EndDate = 1 << 4,
    DqtSpent = 1 << 5,
    DqtSanctionCode = 1 << 6
}
