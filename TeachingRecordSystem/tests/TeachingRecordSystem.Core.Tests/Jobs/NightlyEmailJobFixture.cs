using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[CollectionDefinition(nameof(NightEmailJobCollection), DisableParallelization = true)]
public class NightEmailJobCollection;

public class NightlyEmailJobFixture
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

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataPersonDataSource.CrmAndTrs);

        CrmServiceClientProvider = new TestCrmServiceClientProvider(organizationService);
    }

    public TestableClock Clock { get; }

    public DbFixture DbFixture { get; }

    public ILoggerFactory LoggerFactory { get; }

    public TestData TestData { get; }

    public ICrmServiceClientProvider CrmServiceClientProvider { get; }

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
