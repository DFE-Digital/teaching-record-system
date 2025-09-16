using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Core.Jobs;

public class AllocateTrnsToPersonsWithEypsJob(
    TrsDbContext dbContext,
    ITrnGenerator trnGenerator,
    IFileService fileService,
    IClock clock)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Get all persons where HasEyps has been set to true but there is no TRN
        var personsWithHasEypsAndNoTrn = await dbContext.Persons
            .Where(p => p.HasEyps && p.Trn == null)
            .ToListAsync(cancellationToken);

        var updatedPersons = new List<Guid>();

        // Update all persons where HasEyps has been set to true but there is no TRN
        foreach (var person in personsWithHasEypsAndNoTrn)
        {
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

            updatedPersons.Add(person.PersonId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (!dryRun)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        if (updatedPersons.Count == 0)
        {
            return;
        }

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(updatedPersons.Select(p => new { PersonId = p }), cancellationToken);
        await writer.FlushAsync();
        stream.Position = 0;

        await fileService.UploadFileAsync($"allocatetrntopersonswitheyps{clock.UtcNow:yyyyMMddHHmmss}.csv", stream, "text/csv");
    }
}
