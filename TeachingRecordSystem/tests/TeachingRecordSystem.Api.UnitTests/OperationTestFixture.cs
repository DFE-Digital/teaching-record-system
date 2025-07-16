using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.UnitTests;

public class OperationTestFixture
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly object _createdApplicationUserLock = new();
    private bool _applicationUserCreated;

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

        TestData = new(
            DbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            new FakeTrnGenerator(),
            TestDataPersonDataSource.CrmAndTrs);
    }

    public TestableClock Clock { get; }

    public IServiceProvider Services { get; }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public void EnsureApplicationUser()
    {
        lock (_createdApplicationUserLock)
        {
            if (_applicationUserCreated)
            {
                return;
            }

            var applicationUser = TestData.CreateApplicationUserAsync().GetAwaiter().GetResult();

            Mock.Get(_currentUserProvider)
                .Setup(mock => mock.GetCurrentApplicationUser())
                .Returns((applicationUser.UserId, applicationUser.Name));

            _applicationUserCreated = true;
        }
    }
}
