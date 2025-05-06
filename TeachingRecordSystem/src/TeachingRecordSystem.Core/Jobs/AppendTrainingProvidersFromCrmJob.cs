using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.QueryHandlers;

namespace TeachingRecordSystem.Core.Jobs;

public class AppendTrainingProvidersFromCrmJob(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher, ILogger<AppendTrainingProvidersFromCrmJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        /// get the providers from the CRM that have associated teacher records
        var providersInCrm = await crmQueryDispatcher.ExecuteQueryAsync(
                new MyDummyQuery()); // CML TODO - not sure how these queries are meant to be used, so just calling out that I'm not using it currently

        // get the provider records from Trs
        var providersInTrs = dbContext.TrainingProviders;

        // find providers from crm that aren't in Trs
        var providersToAdd = providersInCrm
            .Where(p => !providersInTrs.Any(t => t.Name == p.Name))
            .Select(s => new DataStore.Postgres.Models.TrainingProvider()
            {
                IsActive = false,
                Name = s.Name,
                TrainingProviderId = Guid.NewGuid(),
                Ukprn = s.dfeta_UKPRN
            })
            .ToList();

        // add to Trs
        providersInTrs.AddRange(providersToAdd);

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
