using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events;

public record class AlertDeletedEvent : EventBase, IEventWithPersonId, IEventWithAlert, IEventWithKey
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required Alert Alert { get; init; }
    public required string? Reason { get; init; }
    public required File? EvidenceFile { get; init; }
}
