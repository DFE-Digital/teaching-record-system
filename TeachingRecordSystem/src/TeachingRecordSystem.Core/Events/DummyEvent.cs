using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Events;

public record DummyEvent : EventBase
{
    public static DummyEvent Create() => new()
    {
        EventId = Guid.NewGuid(),
        CreatedUtc = DateTime.UtcNow,
        RaisedBy = User.SystemUserId
    };
}
