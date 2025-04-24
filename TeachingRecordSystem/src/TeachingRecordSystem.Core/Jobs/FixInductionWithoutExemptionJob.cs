using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

public class FixInductionWithoutExemptionJob(IDbContextFactory<TrsDbContext> dbContextFactory, IClock clock)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var readDbContext = await dbContextFactory.CreateDbContextAsync();
        readDbContext.Database.SetCommandTimeout(Timeout.InfiniteTimeSpan);

        var query = readDbContext.Persons
            .Where(p => p.InductionStatusWithoutExemption == InductionStatus.Failed && p.InductionStatus != InductionStatus.Failed);

        await foreach (var personChunk in query.ToAsyncEnumerable().ChunkAsync(20).WithCancellation(cancellationToken))
        {
            await using var writeDbContext = await dbContextFactory.CreateDbContextAsync();
            
            foreach (var person in personChunk)
            {
                writeDbContext.Attach(person);
                var oldInduction = EventModels.Induction.FromModel(person);

                person.InductionStatusWithoutExemption = InductionStatus.RequiredToComplete;

                if (person.Trn == "0881545")
                {
                    person.InductionStatusWithoutExemption = InductionStatus.InProgress;
                }

                var newInduction = EventModels.Induction.FromModel(person);

                var @event = new PersonInductionUpdatedEvent
                {
                    PersonId = person.PersonId,
                    Induction = newInduction,
                    OldInduction = oldInduction,
                    ChangeReason = null,
                    ChangeReasonDetail = null,
                    EvidenceFile = null,
                    Changes = PersonInductionUpdatedEventChanges.InductionStatusWithoutExemption,
                    EventId = Guid.NewGuid(),
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                };

                writeDbContext.Events.Add(Event.FromEventBase(@event, inserted: null));
            }

            await writeDbContext.SaveChangesAsync();
        }
    }
}
