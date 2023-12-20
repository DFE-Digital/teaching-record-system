using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

[Collection(nameof(TrsDataSyncTestCollection))]
public partial class TrsDataSyncHelperTests
{
    public TrsDataSyncHelperTests(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator)
    {
        DbFixture = dbFixture;
        Clock = new();

        var dbContextFactory = dbFixture.GetDbContextFactory();

        Helper = new TrsDataSyncHelper(
            dbContextFactory,
            organizationService,
            referenceDataCache,
            Clock);

        TestData = new TestData(
            dbContextFactory,
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(Helper));
    }

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }
}
