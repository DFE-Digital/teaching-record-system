using TeachingRecordSystem.Core.Dqt.Models;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncHelperTests
{
    [Fact]
    public async Task SyncEventsAsync_NewRecord_WritesNewRowToDb()
    {
        // Arrange
        var @event = new DqtAnnotationDeletedEvent()
        {
            AnnotationId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            EventId = Guid.NewGuid(),
            RaisedBy = SystemUser.SystemUserId
        };

        var eventInfo = EventInfo.Create(@event);

        var trsEventEntity = new dfeta_TRSEvent()
        {
            dfeta_TRSEventId = @event.EventId,
            dfeta_Payload = eventInfo.Serialize(),
            dfeta_EventName = eventInfo.EventName
        };

        // Act
        await Helper.SyncEventsAsync([trsEventEntity], dryRun: false);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var dbEvent = await dbContext.Events.SingleOrDefaultAsync(p => p.EventId == @event.EventId);
            Assert.NotNull(dbEvent);
            Assert.Equal(@event.GetEventName(), dbEvent.EventName);
            Assert.Equal(@event.CreatedUtc, dbEvent.Created);
            Assert.Equal(Clock.UtcNow, dbEvent.Inserted);
            AssertEx.JsonObjectEquals(@event, EventBase.Deserialize(dbEvent.Payload, dbEvent.EventName));
            Assert.Null(dbEvent.Key);
            Assert.False(dbEvent.Published);
            Assert.Null(dbEvent.PersonId);
        });
    }
}
