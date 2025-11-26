using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateLegacySupportTaskEvents(TrsDbContext dbContext) : IEventHandler<SupportTaskCreatedEvent>
{
    public async Task HandleEventAsync(SupportTaskCreatedEvent @event, ProcessContext processContext)
    {
        var legacyEvent = new LegacyEvents.SupportTaskCreatedEvent
        {
            EventId = @event.EventId,
            CreatedUtc = processContext.Now,
            RaisedBy = processContext.Process.UserId!,
            SupportTask = @event.SupportTask
        };

        dbContext.AddEventWithoutBroadcast(legacyEvent);

        await dbContext.SaveChangesAsync();
    }
}
