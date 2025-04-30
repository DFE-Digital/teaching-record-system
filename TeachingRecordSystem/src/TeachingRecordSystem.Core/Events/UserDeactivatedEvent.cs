using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record UserDeactivatedEvent : EventBase
{
    public required User User { get; init; }
    public string? AdditionalReason { get; init; }
    public string? MoreInformation { get; init; }
    public Guid? EvidenceFileId { get; init; }
}
