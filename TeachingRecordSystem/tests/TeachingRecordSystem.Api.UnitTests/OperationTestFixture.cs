using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Api.UnitTests;

public class OperationTestFixture : IAsyncLifetime
{
    private readonly ICurrentUserProvider _currentUserProvider;

    public OperationTestFixture(
        IServiceProvider serviceProvider,
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        ICurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
        Clock = new TestableClock();
        Services = serviceProvider;
        DbFixture = dbFixture;

        var syncHelper = new TrsDataSyncHelper(
            DbFixture.GetDataSource(),
            organizationService,
            referenceDataCache,
            Clock,
            new TestableAuditRepository(),
            new NullLogger<TrsDataSyncHelper>());

        TestData = new(
            DbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            new FakeTrnGenerator(),
            TestDataSyncConfiguration.Sync(syncHelper));
    }

    public TestableClock Clock { get; }

    public IServiceProvider Services { get; }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public async Task InitializeAsync()
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();

        Mock.Get(_currentUserProvider)
            .Setup(mock => mock.GetCurrentApplicationUser())
            .Returns((applicationUser.UserId, applicationUser.Name));
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
