using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure;

public class SeedLookupData(TrsDbContext dbContext) : IStartupTask
{
    Task IStartupTask.ExecuteAsync()
    {
        return AddTrainingProvidersAsync();
    }

    private async Task AddTrainingProvidersAsync()
    {
        await dbContext.TrainingProviders.AddAsync(new TrainingProvider()
        {
            IsActive = true,
            Name = "TestProviderName",
            TrainingProviderId = Guid.NewGuid(),
            Ukprn = "11111111"
        });
        await dbContext.TrainingProviders.AddAsync(new TrainingProvider()
        {
            IsActive = false,
            Name = "TestProviderNameInactive",
            TrainingProviderId = Guid.NewGuid(),
            Ukprn = "23456789"
        });
        await dbContext.SaveChangesAsync();
    }
}
