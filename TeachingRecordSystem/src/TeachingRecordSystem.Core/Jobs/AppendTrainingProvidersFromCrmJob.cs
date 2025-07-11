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

    private async Task ProcessPagedResultAsync(Account[] providersInCrm)
    {
        var providersInTrs = await dbContext
            .TrainingProviders
            .AsNoTracking()
            .ToListAsync();
        var providersWithNoUkprnToAdd = providersInCrm
            .Where(p => string.IsNullOrEmpty(p.dfeta_UKPRN) && !providersInTrs.Any(t => t.TrainingProviderId == p.Id));
        var uniqueUnmatchedUkprnProviders = providersInCrm
            .Where(p =>
                !string.IsNullOrEmpty(p.dfeta_UKPRN) &&
                !providersInTrs.Any(t => t.Ukprn == p.dfeta_UKPRN) &&
                !providersInTrs.Any(t => t.TrainingProviderId == p.Id))
            .GroupBy(p => p.dfeta_UKPRN)
            .Select(g => g.First());

        // add to Trs
        dbContext.TrainingProviders.AddRange(providersWithNoUkprnToAdd
            .Select(s => new TrainingProvider()
            {
                IsActive = false,
                Name = s.Name,
                TrainingProviderId = s.Id,
                Ukprn = null
            })
            .ToList());
        dbContext.TrainingProviders.AddRange(uniqueUnmatchedUkprnProviders
            .Select(s => new TrainingProvider()
            {
                IsActive = false,
                Name = s.Name,
                TrainingProviderId = s.Id,
                Ukprn = Regex.IsMatch(s.dfeta_UKPRN, ukprnRegex) ? s.dfeta_UKPRN : null
            })
            .ToList());

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var crmQuery = new GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery(PageNumber: 1, Pagesize: 1000);

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

            await ProcessPagedResultAsync(result.Providers);

            if (result.MoreRecords)
            {
                crmQuery = crmQuery with { PageNumber = crmQuery.PageNumber + 1, PagingCookie = result.PagingCookie };
            }
            else
            {
                break;
            }
        }
    }
}
