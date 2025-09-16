using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Core.Jobs;

public class AllocateTrnToPersonJob(
    TrsDbContext dbContext,
    ITrnGenerator trnGenerator,
    IClock clock)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var person = await dbContext.Persons
            .Where(p => p.PersonId == Guid.Parse("5057496c-849f-4433-915f-b1b46e1855e8"))
            .SingleAsync(cancellationToken);

        var newTrn = await trnGenerator.GenerateTrnAsync();
        person.Trn = newTrn;
        dbContext.AddEvent(new TrnAllocatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            PersonId = person.PersonId,
            RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId,
            Trn = newTrn
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        if (!dryRun)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
