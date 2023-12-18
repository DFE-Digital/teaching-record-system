namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

[Collection(nameof(TrsDataSyncTestCollection))]
public partial class TrsDataSyncServiceTests(TrsDataSyncServiceFixture fixture) : IClassFixture<TrsDataSyncServiceFixture>
{
    private TestableClock Clock => fixture.Clock;

    private TestData TestData => fixture.TestData;
}
