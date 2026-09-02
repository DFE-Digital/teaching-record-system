using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using LegacyTrnAllocatedEvent = TeachingRecordSystem.Core.Events.Legacy.TrnAllocatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy
/// <c>TrnAllocatedEvent</c>s stored in the <c>events</c> table. These came from the one-off jobs that allocated
/// TRNs to persons with EYPS and to overseas NPQ applicants; the jobs have since been removed, so this is a
/// closed set with nothing writing the legacy event any more.
/// </summary>
public class BackfillTrnAllocationProcessesJob(TrsDbContext dbContext)
{
    // This matches the EventName value stored in the events table for the legacy event.
    private static readonly string _legacyEventName = typeof(LegacyTrnAllocatedEvent).Name;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Only migrate events that haven't already been back-filled so the job is idempotent.
        var legacyEvents = await dbContext.Events
            .Where(e => e.EventName == _legacyEventName)
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            var eventData = (LegacyTrnAllocatedEvent)legacyEvent.ToEventBase();

            CreateProcessAndProcessEvent(legacyEvent, eventData);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private void CreateProcessAndProcessEvent(Event legacyEvent, LegacyTrnAllocatedEvent trnAllocatedEvent)
    {
        var processId = Guid.NewGuid();

        IEvent newEvent = new TrnAllocatedEvent
        {
            EventId = legacyEvent.EventId,
            PersonId = trnAllocatedEvent.PersonId,
            Trn = trnAllocatedEvent.Trn
        };

        dbContext.Processes.Add(new Process
        {
            ProcessId = processId,
            ProcessType = ProcessType.TrnAllocating,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = trnAllocatedEvent.RaisedBy.UserId,
            DqtUserId = trnAllocatedEvent.RaisedBy.DqtUserId,
            DqtUserName = trnAllocatedEvent.RaisedBy.DqtUserName,
            PersonIds = [.. newEvent.PersonIds],
            OneLoginUserSubjects = [.. newEvent.OneLoginUserSubjects],
            SupportTaskReferences = [.. newEvent.SupportTaskReferences],
            ChangeReason = null
        });

        dbContext.ProcessEvents.Add(new ProcessEvent
        {
            ProcessEventId = newEvent.EventId,
            ProcessId = processId,
            EventName = newEvent.GetType().Name,
            Payload = newEvent,
            PersonIds = newEvent.PersonIds,
            OneLoginUserSubjects = newEvent.OneLoginUserSubjects,
            SupportTaskReferences = newEvent.SupportTaskReferences,
            CreatedOn = legacyEvent.Created
        });
    }
}
