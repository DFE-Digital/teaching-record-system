namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Event
{
    public const string KeyUniqueIndexName = "ix_events_key";

    public required Guid EventId { get; init; }
    public required string EventName { get; init; }
    public required DateTime Created { get; init; }
    public required DateTime Inserted { get; init; }
    public required string Payload { get; init; }
    public bool Published { get; set; }
    public Guid? PersonId { get; init; }
    public Guid? QualificationId { get; init; }
    public Guid? AlertId { get; init; }

    public static Event FromEventBase(EventBase @event, DateTime? inserted)
    {
        var eventName = @event.GetEventName();
        var payload = @event.Serialize();

        return new Event()
        {
            EventId = @event.EventId,
            EventName = eventName,
            Created = @event.CreatedUtc,
            Inserted = inserted ?? @event.CreatedUtc,
            Payload = payload,
            PersonId = @event.TryGetPersonId(out var personId) ? personId : null,
            QualificationId =
                (@event as IEventWithMandatoryQualification)?.MandatoryQualification.QualificationId ??
                (@event as IEventWithRouteToProfessionalStatus)?.RouteToProfessionalStatus.QualificationId,
            AlertId = (@event as IEventWithAlert)?.Alert.AlertId
        };
    }

    public EventBase ToEventBase()
    {
        return EventBase.Deserialize(Payload, EventName);
    }
}
