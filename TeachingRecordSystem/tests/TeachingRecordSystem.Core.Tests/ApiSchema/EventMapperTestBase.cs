using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.ApiSchema;

public abstract class EventMapperTestBase(EventMapperFixture fixture)
{
    public TestableClock Clock => fixture.Clock;

    public DbFixture DbFixture => fixture.DbFixture;

    public ReferenceDataCache ReferenceDataCache => fixture.ReferenceDataCache;

    public TestData TestData => fixture.TestData;

    public async Task WithEventMapper<TMapper>(Func<TMapper, Task> action)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var mapper = ActivatorUtilities.CreateInstance<TMapper>(scope.ServiceProvider);
        await action(mapper);
    }
}

public class EventMapperFixture
{
    public EventMapperFixture(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ICrmQueryDispatcher crmQueryDispatcher,
        FakeTrnGenerator trnGenerator,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    {
        Clock = new TestableClock();
        DbFixture = dbFixture;
        ReferenceDataCache = new ReferenceDataCache(crmQueryDispatcher, dbFixture.GetDbContextFactory());

        var syncHelper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            organizationService,
            ReferenceDataCache,
            Clock,
            new TestableAuditRepository(),
            loggerFactory.CreateLogger<TrsDataSyncHelper>(),
            BlobStorageFileService.Object,
            configuration);

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            ReferenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(syncHelper));

        Services = serviceProvider;
    }

    public TestableClock Clock { get; }

    public DbFixture DbFixture { get; }

    public ReferenceDataCache ReferenceDataCache { get; set; }

    public TestData TestData { get; }

    public IServiceProvider Services { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();
}
