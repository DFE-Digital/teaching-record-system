using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record SupportTaskCreatedEvent : EventBase, IEventWithOptionalPersonId
{
    public required SupportTask SupportTask { get; init; }

    [JsonIgnore]
    public Guid? PersonId => SupportTask.PersonId;
}
