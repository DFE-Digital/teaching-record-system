using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure;

public class SeedLookupData(IDbContextFactory<TrsDbContext> dbContextFactory) : IStartupTask
{
    Task IStartupTask.ExecuteAsync()
    {
        return AddTrainingProvidersAsync();
    }

    public static async Task ResetTrainingProvidersAsync(TrsDbContext dbContext)
    {
        await dbContext.TrainingProviders.ExecuteDeleteAsync();

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

        dbContext.TrainingProviders.Add(new TrainingProvider()
        {
            IsActive = true,
            Name = "Non-UK establishment",
            TrainingProviderId = Guid.NewGuid(),
            Ukprn = null
        });

        dbContext.TrainingProviders.Add(new TrainingProvider()
        {
            IsActive = true,
            Name = "UK establishment (Scotland/Northern Ireland)",
            TrainingProviderId = Guid.NewGuid(),
            Ukprn = null
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task AddTrainingProvidersAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (await dbContext.TrainingProviders.AnyAsync())
        {
            return;
        }

        await ResetTrainingProvidersAsync(dbContext);
    }
}
