using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record AlertUpdatedEvent : EventBase, IEventWithPersonId, IEventWithAlert
{
    public string? Key { get; init; }
    public required Guid PersonId { get; init; }
    public required Alert Alert { get; init; }
    public required Alert OldAlert { get; init; }
    public required string? ChangeReason { get; init; }
    public required string? ChangeReasonDetail { get; init; }
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
    DqtSpent = 1 << 5,
    DqtSanctionCode = 1 << 6
}

public static class AlertUpdatedEventChangesExtensions
{
    public static Events.AlertUpdatedEventChanges ToAlertUpdatedEventChanges(this AlertUpdatedEventChanges changes) =>
        Events.AlertUpdatedEventChanges.None
        | (changes.HasFlag(AlertUpdatedEventChanges.Details) ? Events.AlertUpdatedEventChanges.Details : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.ExternalLink) ? Events.AlertUpdatedEventChanges.ExternalLink : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.StartDate) ? Events.AlertUpdatedEventChanges.StartDate : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.EndDate) ? Events.AlertUpdatedEventChanges.EndDate : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.DqtSpent) ? Events.AlertUpdatedEventChanges.DqtSpent : 0)
        | (changes.HasFlag(AlertUpdatedEventChanges.DqtSanctionCode) ? Events.AlertUpdatedEventChanges.DqtSanctionCode : 0);
}
