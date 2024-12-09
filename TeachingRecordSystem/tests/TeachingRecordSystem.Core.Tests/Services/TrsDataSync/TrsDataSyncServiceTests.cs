
namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

[Collection(nameof(TrsDataSyncTestCollection))]
public partial class TrsDataSyncServiceTests(TrsDataSyncServiceFixture fixture) : IClassFixture<TrsDataSyncServiceFixture>, IAsyncLifetime
{
    private TestableClock Clock => fixture.Clock;

    private TestData TestData => fixture.TestData;

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.InitializeAsync() => fixture.DbFixture.DbHelper.ClearDataAsync();
}
