using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class DeleteStaleJourneyStatesJobTests(DbFixture dbFixture) : IAsyncLifetime
{
    private TrsDbContext _trsContext = null!;

    public TestableClock Clock { get; } = new();

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync()
    {
        _trsContext = await dbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
    }

    [Fact]
    public async Task DeleteStaleJourneyStatesJob_RemovesJourneyStatesOlderThanOneDay_UpdatesMetadataLastRunDate()
    {
        // Arrange
        var journeyState1 = new Core.DataStore.Postgres.Models.JourneyState
        {
            InstanceId = Guid.NewGuid().ToString(),
            Created = Clock.UtcNow.AddDays(-2),
            State = "State1",
            UserId = Guid.NewGuid().ToString(),
            Updated = Clock.UtcNow.AddDays(-2)
        };
        var journeyState2 = new Core.DataStore.Postgres.Models.JourneyState
        {
            InstanceId = Guid.NewGuid().ToString(),
            Created = Clock.UtcNow.AddDays(-1).AddMinutes(Random.Shared.Next(0, 60)),
            State = "State2",
            UserId = Guid.NewGuid().ToString(),
            Updated = Clock.UtcNow.AddDays(-1).AddMinutes(Random.Shared.Next(0, 60))
        };
        var journeyState3 = new Core.DataStore.Postgres.Models.JourneyState
        {
            InstanceId = Guid.NewGuid().ToString(),
            Created = Clock.UtcNow.AddMinutes(Random.Shared.Next(0, 60)),
            State = "State3",
            UserId = Guid.NewGuid().ToString(),
            Updated = Clock.UtcNow.AddMinutes(Random.Shared.Next(0, 60))
        };

        _trsContext.JourneyStates.Add(journeyState1);
        _trsContext.JourneyStates.Add(journeyState2);
        _trsContext.JourneyStates.Add(journeyState3);
        await _trsContext.SaveChangesAsync();

        var job = new DeleteStaleJourneyStatesJob(_trsContext, Clock);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        var expected = new JourneyState[] { journeyState2, journeyState3 };
        var remainingJourneyStates = await _trsContext.JourneyStates.ToListAsync();
        Assert.All(remainingJourneyStates, s => Assert.Contains(s.InstanceId, expected.Select(e => e.InstanceId)));
        Assert.Equal(2, remainingJourneyStates.Count);
    }
}
