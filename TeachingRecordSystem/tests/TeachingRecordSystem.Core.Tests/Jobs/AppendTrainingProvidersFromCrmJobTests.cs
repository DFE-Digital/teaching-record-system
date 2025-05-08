using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class AppendTrainingProvidersFromCrmJobTests : IAsyncLifetime
{
    public AppendTrainingProvidersFromCrmJobTests(
        DbFixture dbFixture)
    {
        DbFixture = dbFixture!;
    }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();
    private TrsDbContext _trsContext = null!;

    private Mock<ICrmQueryDispatcher> _crmQueryDispatcherMock = new Mock<ICrmQueryDispatcher>();

    public async Task InitializeAsync()
    {
        _trsContext = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
    }

    public DbFixture DbFixture { get; }
    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Execute_WithTrainingProviderInCrm_UpdatesDb()
    {
        // arrange
        var accounts = new List<Account>()
        {
            new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Provider2",
                AccountNumber = "123",
                dfeta_UKPRN = "12345672"
            },
            new Account()
            {
                Id = Guid.NewGuid(),
                Name = "A provider with same Ukprn as above but a different name",
                AccountNumber = "123",
                dfeta_UKPRN = "12345672"
            },
            new Account()
            {
                Id = Guid.NewGuid(),
                Name = "A provider with same Ukprn as in TrsDB",
                AccountNumber = "123",
                dfeta_UKPRN = "12345671"
            }
        };

        _crmQueryDispatcherMock
            .Setup(x => x.ExecuteQueryAsync(It.IsAny<ICrmQuery<Account[]>>()))
            .ReturnsAsync(accounts.ToArray());

        // training provider data into Trs
        var provider = _trsContext.TrainingProviders.Add(new TrainingProvider() { IsActive = true, Name = "Provider1", Ukprn = "12345671", TrainingProviderId = Guid.NewGuid() });
        _trsContext.SaveChanges();

        var JobUnderTest = new AppendTrainingProvidersFromCrmJob(_trsContext, _crmQueryDispatcherMock.Object);

        // act
        await JobUnderTest.ExecuteAsync(new CancellationToken());

        // assert
        var appendedProviderList = _trsContext.TrainingProviders?.AsEnumerable();
        Assert.Equal(2, appendedProviderList!.Count());
        Assert.NotNull(appendedProviderList!.Single(p => p.Ukprn == "12345672").Name);
    }
}
