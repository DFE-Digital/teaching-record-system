namespace TeachingRecordSystem.Core.Events;

public record AlertUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    public required Guid PersonId { get; init; }
    public required EventModels.Alert Alert { get; init; }
    public required EventModels.Alert OldAlert { get; init; }
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

public static class AlertUpdatedEventExtensions
{
    public static LegacyEvents.AlertUpdatedEventChanges ToLegacyAlertUpdatedEventChanges(this AlertUpdatedEventChanges changes) =>
        LegacyEvents.AlertUpdatedEventChanges.None
        | (changes.HasFlag(AlertUpdatedEventChanges.Details) ? LegacyEvents.AlertUpdatedEventChanges.Details : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.ExternalLink) ? LegacyEvents.AlertUpdatedEventChanges.ExternalLink : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.StartDate) ? LegacyEvents.AlertUpdatedEventChanges.StartDate : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.EndDate) ? LegacyEvents.AlertUpdatedEventChanges.EndDate : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.DqtSpent) ? LegacyEvents.AlertUpdatedEventChanges.DqtSpent : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.DqtSanctionCode) ? LegacyEvents.AlertUpdatedEventChanges.DqtSanctionCode : 0);
}
