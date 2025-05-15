using System.ServiceModel;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Jobs;

public class AppendTrainingProvidersFromCrmJob(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher)
{
    private const string ukprnRegex = @"^\d{8}$";

    private void ProcessPagedResult(Account[] providersInCrm, List<TrainingProvider> providersInTrs)
    {
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
                Ukprn = Regex.IsMatch(s.dfeta_UKPRN, ukprnRegex) ? s.dfeta_UKPRN : null
            })
            .ToList());
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var crmQuery = new GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery(pageNumber: 1);
        var providersInTrs = await dbContext.TrainingProviders.ToListAsync();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PagedProviderResults result;
            try
            {
                result = await crmQueryDispatcher.ExecuteQueryAsync(crmQuery);
            }
            catch (FaultException<OrganizationServiceFault> e) when (e.IsCrmRateLimitException(out var retryAfter))
            {
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            ProcessPagedResult(result.Providers, providersInTrs);

            if (result.MoreRecords)
            {
                crmQuery = crmQuery with { pageNumber = crmQuery.pageNumber + 1, pagingCookie = result.PagingCookie };
                providersInTrs = dbContext.TrainingProviders.Local.ToList();
            }
            else
            {
                if (dbContext.ChangeTracker.HasChanges())
                {
                    await dbContext.SaveChangesAsync();
                }
                break;
            }
        }

    }
}
