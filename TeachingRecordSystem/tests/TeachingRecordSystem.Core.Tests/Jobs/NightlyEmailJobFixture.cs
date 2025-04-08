using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.DqtNoteAttachments;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[CollectionDefinition(nameof(NightEmailJobCollection), DisableParallelization = true)]
public class NightEmailJobCollection
{
}

public class NightlyEmailJobFixture : IAsyncLifetime
{
    public NightlyEmailJobFixture(
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
            loggerFactory.CreateLogger<TrsDataSyncHelper>(),
            DqtNoteFileAttachmentStorageMock.Object);

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

    public Mock<IDqtNoteAttachmentStorage> DqtNoteFileAttachmentStorageMock { get; } = new Mock<IDqtNoteAttachmentStorage>();

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
