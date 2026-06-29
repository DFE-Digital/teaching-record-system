using System.Diagnostics.CodeAnalysis;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.ChangeHistoryEntry;

public record ChangeHistoryEntryViewModel
{
    public required ChangeHistoryViewType ViewType { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string UserName { get; init; }
    public required Guid ProcessId { get; init; }
    public required ProcessType ProcessType { get; init; }
    public required IChangeReasonInfo? ChangeReason { get; init; }
    public required IReadOnlyCollection<IEvent> Events { get; init; }

    public bool ContainsEvent<T>() where T : IEvent => Events.OfType<T>().Any();

    public T GetEvent<T>() where T : IEvent => Events.OfType<T>().Single();

    public bool TryGetEvent<T>([NotNullWhen(true)] out T? @event) where T : IEvent
    {
        @event = Events.OfType<T>().SingleOrDefault();
        return @event is not null;
    }
}
