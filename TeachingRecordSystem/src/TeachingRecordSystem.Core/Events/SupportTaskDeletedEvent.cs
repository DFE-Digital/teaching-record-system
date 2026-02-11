using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core.Events;

public record SupportTaskDeletedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => IEvent.CoalescePersonIds(SupportTask.PersonId);
    string[] IEvent.OneLoginUserSubjects => [];
    [JsonIgnore]
    public Guid? PersonId => SupportTask.PersonId;
    public required string SupportTaskReference { get; init; }
    public required EventModels.SupportTask SupportTask { get; init; }
    public required string? ReasonDetail { get; init; }
}
