using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core.Events;

public record SupportTaskUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => IEvent.CoalescePersonIds(SupportTask.PersonId);
    string[] IEvent.OneLoginUserSubjects => [];
    [JsonIgnore]
    public Guid? PersonId => SupportTask.PersonId;
    public required string SupportTaskReference { get; init; }
    public required SupportTaskUpdatedEventChanges Changes { get; init; }
    public required EventModels.SupportTask SupportTask { get; init; }
    public required EventModels.SupportTask OldSupportTask { get; init; }
    public required string? Comments { get; init; }
    public required string? RejectionReason { get; init; }
}

[Flags]
public enum SupportTaskUpdatedEventChanges
{
    None = 0,
    Status = 1 << 0,
    Data = 1 << 1,
    ResolveJourneySavedState = 1 << 2
}
