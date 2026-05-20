using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record UserDeactivatedEvent : EventBase
{
    public required User User { get; init; }
    public string? DeactivatedReason { get; init; }
    public string? DeactivatedReasonDetail { get; init; }
    public Guid? EvidenceFileId { get; init; }
}
