using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure;

public class SeedLookupData(IDbContextFactory<TrsDbContext> dbContextFactory) : IStartupTask
{
    Task IStartupTask.ExecuteAsync()
    {
        return AddTrainingProvidersAsync();
    }

    private async Task AddTrainingProvidersAsync()
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        if (await dbContext.TrainingProviders.AnyAsync())
        {
            return;
        }

        dbContext.TrainingProviders.Add(new TrainingProvider()
        {
            IsActive = true,
            Name = "TestProviderName",
            TrainingProviderId = Guid.NewGuid(),
            Ukprn = "11111111"
        });

        dbContext.TrainingProviders.Add(new TrainingProvider()
        {
            IsActive = false,
            Name = "TestProviderNameInactive",
            TrainingProviderId = Guid.NewGuid(),
            Ukprn = "23456789"
        });

        await dbContext.SaveChangesAsync();
    }
}
