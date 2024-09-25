using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events;

public record class AlertUpdatedEvent : EventBase, IEventWithPersonId, IEventWithAlert
{
    public required Guid PersonId { get; init; }
    public required Alert Alert { get; init; }
    public required Alert OldAlert { get; init; }
    public required string? ChangeReason { get; init; }
    public required File? EvidenceFile { get; init; }
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
}
