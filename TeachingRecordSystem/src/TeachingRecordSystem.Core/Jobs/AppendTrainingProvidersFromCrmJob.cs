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

        // find providers from crm that aren't in Trs
        var providersToAdd = providersInCrm
            .Where(p =>
                !providersInTrs.Any(t => t.Ukprn == p.dfeta_UKPRN) &&
                !providersInTrs.Any(t => string.IsNullOrEmpty(p.dfeta_UKPRN) && string.Equals(t.Name, p.Name, StringComparison.OrdinalIgnoreCase)))
            .Select(s => new DataStore.Postgres.Models.TrainingProvider()
            {
                IsActive = false,
                Name = s.Name,
                TrainingProviderId = Guid.NewGuid(),
                Ukprn = s.dfeta_UKPRN
            })
            .ToList();

        // add to Trs
        dbContext.TrainingProviders.AddRange(providersToAdd);

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
