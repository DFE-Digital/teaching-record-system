using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Events.EventHandlers;

public class SaveEventToDbEventHandler(TrsDbContext dbContext) : IEventHandler
{
    public Task HandleAsync(EventBase @event)
    {
        dbContext.AddEvent(@event);
        return dbContext.SaveChangesAsync();
    }
}
