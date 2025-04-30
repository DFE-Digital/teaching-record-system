using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Services.DqtNoteAttachments;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.PersonMatching;

[Collection(nameof(DisableParallelization))]
public partial class PersonMatchingServiceTests : IAsyncLifetime
{
    public PersonMatchingServiceTests(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        ILoggerFactory loggerFactory)
    {
        DbFixture = dbFixture;
        Clock = new();

        var syncHelper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            organizationService,
            referenceDataCache,
            Clock,
            new TestableAuditRepository(),
            loggerFactory.CreateLogger<TrsDataSyncHelper>(),
            DqtNoteFileAttachment.Object);

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(syncHelper));
    }

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public Task InitializeAsync() => DbFixture.DbHelper.ClearDataAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    public Mock<IDqtNoteAttachmentStorage> DqtNoteFileAttachment { get; } = new Mock<IDqtNoteAttachmentStorage>();
}
