using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Jobs;

public class AppendTrainingProvidersFromCrmJob(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        /// get the providers from the CRM that have associated teacher records
        var providersInCrm = await crmQueryDispatcher.ExecuteQueryAsync(
                new GetAllIttProvidersWithCorrespondingIttRecordsQuery());

        // get the provider records from Trs
        var providersInTrs = await dbContext.TrainingProviders.ToListAsync();

        var emptyUkprnProvidersToAdd = providersInCrm
            .Where(p => string.IsNullOrEmpty(p.dfeta_UKPRN) && !providersInTrs.Any(t => t.TrainingProviderId == p.Id));
        var uniqueUnmatchedUkprnProviders = providersInCrm
            .Where(p =>
                !string.IsNullOrEmpty(p.dfeta_UKPRN) &&
                !providersInTrs.Any(t => t.Ukprn == p.dfeta_UKPRN))
            .GroupBy(p => p.dfeta_UKPRN)
            .Select(g => g.First());

        // add to Trs
        dbContext.TrainingProviders.AddRange(emptyUkprnProvidersToAdd
            .Select(s => new DataStore.Postgres.Models.TrainingProvider()
            {
                IsActive = false,
                Name = s.Name,
                TrainingProviderId = s.Id,
                Ukprn = s.dfeta_UKPRN
            })
            .ToList());
        dbContext.TrainingProviders.AddRange(uniqueUnmatchedUkprnProviders
            .Select(s => new DataStore.Postgres.Models.TrainingProvider()
            {
                IsActive = false,
                Name = s.Name,
                TrainingProviderId = s.Id,
                Ukprn = s.dfeta_UKPRN
            })
            .ToList());

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
