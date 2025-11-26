using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<Process> CreateProcessAsync(
        ProcessType processType,
        Guid? userId = null,
        params IEvent[] events)
    {
        if (events.Length == 0)
        {
            throw new ArgumentException("At least one event must be provided to create a process.", nameof(events));
        }

        return WithDbContextAsync(async dbContext =>
        {
            var process = new Process
            {
                ProcessId = Guid.NewGuid(),
                ProcessType = processType,
                CreatedOn = Clock.UtcNow,
                UpdatedOn = Clock.UtcNow,
                UserId = userId ?? SystemUser.SystemUserId,
                DqtUserId = null,
                DqtUserName = null,
                PersonIds = events.SelectMany(e => e.PersonIds).Distinct().ToList()
            };

            dbContext.Processes.Add(process);

            foreach (var @event in events)
            {
                dbContext.Set<ProcessEvent>().Add(new ProcessEvent
                {
                    ProcessEventId = @event.EventId,
                    ProcessId = process.ProcessId,
                    EventName = @event.GetType().Name,
                    Payload = @event,
                    PersonIds = @event.PersonIds,
                    CreatedOn = Clock.UtcNow
                });
            }

            await dbContext.SaveChangesAsync();

            return await dbContext.Processes.SingleAsync(p => p.ProcessId == process.ProcessId);
        });
    }
}
