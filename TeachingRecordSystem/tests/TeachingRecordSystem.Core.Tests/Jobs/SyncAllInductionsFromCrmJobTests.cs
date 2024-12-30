using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[CollectionDefinition(nameof(TrsDataSyncTestCollection), DisableParallelization = true)]
public class SyncAllInductionsFromCrmJobTests : SyncFromCrmJobTestBase, IAsyncLifetime
{
    public SyncAllInductionsFromCrmJobTests(SyncFromCrmJobFixture jobFixture) : base(jobFixture)
    {
    }

    [Fact(Skip = "Causes deadlock on CI for some reason")]
    public async Task SyncInductionsAsync_WithExistingDqtInduction_UpdatesPersonRecord()
    {
        // Arrange
        var inductionStatus = dfeta_InductionStatus.Pass;
        var inductionStartDate = Clock.Today.AddYears(-1);
        var inductionCompletedDate = Clock.Today.AddDays(-5);
        var options = Options.Create(new TrsDataSyncServiceOptions()
        {
            CrmConnectionString = "dummy",
            ModelTypes = [TrsDataSyncHelper.ModelTypes.Person],
            PollIntervalSeconds = 60,
            IgnoreInvalidData = false,
            RunService = false
        });

        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithQts()
                .WithDqtInduction(inductionStatus, null, inductionStartDate, inductionCompletedDate)
                .WithSyncOverride(false));

        // Act
        var job = new SyncAllInductionsFromCrmJob(
            CrmServiceClientProvider,
            Helper,
            options,
            LoggerFactory.CreateLogger<SyncAllInductionsFromCrmJob>());

        await job.ExecuteAsync(createMigratedEvent: false, dryRun: false, CancellationToken.None);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == person.ContactId);
            Assert.Equal(inductionStatus.ToInductionStatus(), updatedPerson!.InductionStatus);
            Assert.Equal(inductionStartDate, updatedPerson.InductionStartDate);
            Assert.Equal(inductionCompletedDate, updatedPerson.InductionCompletedDate);
        });
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.InitializeAsync() => JobFixture.DbFixture.DbHelper.ClearDataAsync();
}
