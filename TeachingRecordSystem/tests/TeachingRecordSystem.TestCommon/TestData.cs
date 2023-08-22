using Microsoft.EntityFrameworkCore;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData : CrmTestData
{
    private static int _lastTrn = 4000000;

    public TestData(
        IDbContextFactory<TrsDbContext> dbContextFactory,
        IOrganizationServiceAsync organizationService,
        ReferenceDataCache referenceDataCache)
        : base(organizationService, referenceDataCache, generateTrn: () => throw new NotImplementedException())
    {
        DbContextFactory = dbContextFactory;
    }

    protected IDbContextFactory<TrsDbContext> DbContextFactory { get; }

    public override Task<string> GenerateTrn() => Task.FromResult(Interlocked.Increment(ref _lastTrn).ToString());

    protected async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    protected async Task WithDbContext(Func<TrsDbContext, Task> action)
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await action(dbContext);
    }
}
