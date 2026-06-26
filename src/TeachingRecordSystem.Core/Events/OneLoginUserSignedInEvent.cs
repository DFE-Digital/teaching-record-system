using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core.Events;

public record OneLoginUserSignedInEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => IEvent.CoalescePersonIds(OneLoginUser.PersonId);
    public string[] OneLoginUserSubjects => [OneLoginUser.Subject];
    string[] IEvent.SupportTaskReferences => [];
    [JsonIgnore]
    public Guid? PersonId => OneLoginUser.PersonId;
    public required EventModels.OneLoginUser OneLoginUser { get; init; }
}
