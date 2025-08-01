using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record DqtInductionImportedEvent : EventBase, IEventWithPersonId
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required DqtInduction Induction { get; init; }
    public required int DqtState { get; init; }
}
