using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core.Events;

public record SupportTaskCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => IEvent.CoalescePersonIds(SupportTask.PersonId);
    string[] IEvent.OneLoginUserSubjects => [];
    [JsonIgnore]
    public Guid? PersonId => SupportTask.PersonId;
    public required EventModels.SupportTask SupportTask { get; init; }
}
