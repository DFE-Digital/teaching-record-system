using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class SyncFromCrmJobFixture : IAsyncLifetime
{
    public SyncFromCrmJobFixture(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        ILoggerFactory loggerFactory)
    {
        DbFixture = dbFixture;
        LoggerFactory = loggerFactory;
        Clock = new();
        LoggerFactory = loggerFactory;

        Helper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            organizationService,
            referenceDataCache,
            Clock,
            new TestableAuditRepository(),
            loggerFactory.CreateLogger<TrsDataSyncHelper>());

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(Helper));

        CrmServiceClientProvider = new TestCrmServiceClientProvider(organizationService);
    }

    public TestableClock Clock { get; }

    public DbFixture DbFixture { get; }

    public ILoggerFactory LoggerFactory { get; }

    public TrsDataSyncHelper Helper { get; }

    public TestData TestData { get; }

    public ICrmServiceClientProvider CrmServiceClientProvider { get; }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    private class TestCrmServiceClientProvider : ICrmServiceClientProvider
    {
        private readonly IOrganizationServiceAsync2 _organizationService;

        public TestCrmServiceClientProvider(IOrganizationServiceAsync2 organizationService)
        {
            _organizationService = organizationService;
        }

        public IOrganizationServiceAsync2 GetClient(string name)
        {
            return _organizationService;
        }
    }
}
