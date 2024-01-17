namespace TeachingRecordSystem.Core.Events;

public record DummyEvent : EventBase
{
    public static DummyEvent Create() => new()
    {
        EventId = Guid.NewGuid(),
        CreatedUtc = DateTime.UtcNow,
        RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
    };
}
