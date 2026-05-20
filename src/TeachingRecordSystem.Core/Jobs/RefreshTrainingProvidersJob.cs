using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.PublishApi;

namespace TeachingRecordSystem.Core.Jobs;

public class RefreshTrainingProvidersJob(IPublishApiClient publishApiClient, IDbContextFactory<TrsDbContext> dbContextFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var providers = await publishApiClient.GetAccreditedProvidersAsync();
        using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        foreach (var item in providers)
        {
            var existingProvider = await dbContext.TrainingProviders.SingleOrDefaultAsync(p => p.Ukprn == item.Attributes.Ukprn, cancellationToken: cancellationToken);
            if (existingProvider == null)
            {
                dbContext.TrainingProviders.Add(new()
                {
                    TrainingProviderId = Guid.NewGuid(),
                    Ukprn = item.Attributes.Ukprn,
                    Name = item.Attributes.Name,
                    IsActive = true
                });
            }
            else
            {
                existingProvider.Name = item.Attributes.Name;
                existingProvider.IsActive = true;
            }
        }

        foreach (var existingProvider in await dbContext.TrainingProviders.ToListAsync(cancellationToken: cancellationToken))
        {
            if (!providers.Any(p => p.Attributes.Ukprn == existingProvider.Ukprn))
            {
                existingProvider.IsActive = false;
            }
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
