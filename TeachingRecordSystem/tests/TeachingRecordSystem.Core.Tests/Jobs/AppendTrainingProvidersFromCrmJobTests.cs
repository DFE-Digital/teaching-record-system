using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class AppendTrainingProvidersFromCrmJobTests : IAsyncLifetime
{
    public AppendTrainingProvidersFromCrmJobTests(
        DbFixture dbFixture)
    {
        DbFixture = dbFixture!;
    }

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
        var guidAlreadyInTrs = Guid.NewGuid();
        var accounts = new List<Account>()
        {
            new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Provider2",
                dfeta_UKPRN = "12345672"
            },
            new Account()
            {
                Id = Guid.NewGuid(),
                Name = "A provider with same Ukprn as above but a different name",
                dfeta_UKPRN = "12345672"
            },
            new Account()
            {
                Id = Guid.NewGuid(),
                Name = "A provider with same Ukprn as in TrsDB",
                dfeta_UKPRN = "12345671"
            },
            new Account()
            {
                Id = Guid.NewGuid(),
                Name = "provider 1 with null ukprn",
                dfeta_UKPRN = null
            },
            new Account()
            {
                Id = Guid.NewGuid(),
                Name = "provider 2 with null ukprn",
                dfeta_UKPRN = null
            },
            new Account()
            {
                Id = guidAlreadyInTrs,
                Name = "provider 3 with null ukprn already in Trs",
                dfeta_UKPRN = null
            },
            new Account()
            {
                Id = Guid.NewGuid(),
                Name = "provider with invalid ukprn",
                dfeta_UKPRN = "1234"
            }
        };
        var crmQuery1 = new GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery(pageNumber: 1, It.IsAny<int>(), It.IsAny<string>());
        var crmQuery2 = new GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery(pageNumber: 2, It.IsAny<int>(), It.IsAny<string>());
        var crmQuery3 = new GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery(pageNumber: 3, It.IsAny<int>(), It.IsAny<string>());

        _crmQueryDispatcherMock
            .Setup(x => x.ExecuteQueryAsync(crmQuery1))
            .ReturnsAsync(new PagedProviderResults(
                Providers: accounts.ToArray()[..3],
                MoreRecords: true,
                PagingCookie: null));
        _crmQueryDispatcherMock
            .Setup(x => x.ExecuteQueryAsync(crmQuery2))
            .ReturnsAsync(new PagedProviderResults(
                Providers: accounts.ToArray()[3..6],
                MoreRecords: true,
                PagingCookie: null));
        _crmQueryDispatcherMock
            .Setup(x => x.ExecuteQueryAsync(crmQuery3))
            .ReturnsAsync(new PagedProviderResults(
                Providers: accounts.ToArray()[6..],
                MoreRecords: false,
                PagingCookie: null));

        // training provider data into Trs
        _trsContext.TrainingProviders.Add(new TrainingProvider() { IsActive = true, Name = "Provider1", Ukprn = "12345671", TrainingProviderId = Guid.NewGuid() });
        _trsContext.TrainingProviders.Add(new TrainingProvider() { IsActive = true, Name = "Provider2", Ukprn = "12345673", TrainingProviderId = guidAlreadyInTrs });
        _trsContext.SaveChanges();

        var JobUnderTest = new AppendTrainingProvidersFromCrmJob(_trsContext, _crmQueryDispatcherMock.Object);

        // act
        await JobUnderTest.ExecuteAsync(new CancellationToken());

        // assert
        var appendedProviderList = _trsContext.TrainingProviders?.AsEnumerable();
        Assert.Single(appendedProviderList!.Where(p => p.Ukprn == "12345672"));
        Assert.Single(appendedProviderList!.Where(p => p.Name == "provider 1 with null ukprn"));
        Assert.Single(appendedProviderList!.Where(p => p.Name == "provider 2 with null ukprn"));
        Assert.Single(appendedProviderList!.Where(p => p.Ukprn == "12345671"));
        Assert.Null(appendedProviderList!.Single(p => p.Name == "provider with invalid ukprn").Ukprn);
    }
}
