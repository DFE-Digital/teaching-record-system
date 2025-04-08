using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.Services.DqtNoteAttachments;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Api.UnitTests;

public class OperationTestFixture
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
            new NullLogger<TrsDataSyncHelper>(),
            DqtNoteFileAttachment.Object);

        TestData = new(
            DbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            new FakeTrnGenerator(),
            TestDataSyncConfiguration.Sync(syncHelper));

        var applicationUser = TestData.CreateApplicationUserAsync().GetAwaiter().GetResult();

        Mock.Get(_currentUserProvider)
            .Setup(mock => mock.GetCurrentApplicationUser())
            .Returns((applicationUser.UserId, applicationUser.Name));
    }

    public TestableClock Clock { get; }

    public IServiceProvider Services { get; }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public Mock<IDqtNoteAttachmentStorage> DqtNoteFileAttachment { get; } = new Mock<IDqtNoteAttachmentStorage>();
}
