using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure;

public class SeedLookupData(IDbContextFactory<TrsDbContext> dbContextFactory) : IStartupTask
{
    Task IStartupTask.ExecuteAsync()
    {
        return AddTrainingProvidersAsync();
    }

    public static async Task EnsureTestTrainingProvidersAsync(TrsDbContext dbContext)
    {
        TrainingProvider[] testProviders = [
            new TrainingProvider()
            {
                IsActive = true,
                Name = "TestProviderName",
                TrainingProviderId = new("2ad74790-0b31-405a-82c7-3ba6c7cc85b5"),
                Ukprn = "11111111"
            },
            new TrainingProvider()
            {
                IsActive = false,
                Name = "TestProviderNameInactive",
                TrainingProviderId = new("cb942d4d-0922-47a3-ad41-c12f5757c6a0"),
                Ukprn = "23456789"
            },
            new TrainingProvider()
            {
                IsActive = true,
                Name = "Non-UK establishment",
                TrainingProviderId = new("1f9b4093-fac1-4b5c-bf52-608bdd79cf0e"),
                Ukprn = null
            },
            new TrainingProvider()
            {
                IsActive = true,
                Name = "UK establishment (Scotland/Northern Ireland)",
                TrainingProviderId = new("6f0db415-e869-40b2-8fb2-3b040c69f9ce"),
                Ukprn = null
            }
        ];

        var testProviderIds = testProviders.Select(p => p.TrainingProviderId).ToHashSet();

        var existingProviderIds = await dbContext.TrainingProviders
            .Where(p => testProviderIds.Contains(p.TrainingProviderId))
            .Select(p => p.TrainingProviderId)
            .ToListAsync();

        dbContext.TrainingProviders.AddRange(testProviders.Where(p => !existingProviderIds.Contains(p.TrainingProviderId)));

        await dbContext.SaveChangesAsync();
    }

    private async Task AddTrainingProvidersAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (await dbContext.TrainingProviders.AnyAsync())
        {
            return;
        }

        await EnsureTestTrainingProvidersAsync(dbContext);
    }
}
