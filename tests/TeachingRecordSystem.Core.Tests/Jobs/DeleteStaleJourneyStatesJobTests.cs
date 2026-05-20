using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class DeleteStaleJourneyStatesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task DeleteStaleJourneyStatesJob_RemovesJourneyStatesOlderThanOneDay_UpdatesMetadataLastRunDate()
    {
        // Arrange
        var journeyState1 = new JourneyState
        {
            InstanceId = Guid.NewGuid().ToString(),
            Created = Clock.UtcNow.AddDays(-2),
            State = "State1",
            UserId = Guid.NewGuid().ToString(),
            Updated = Clock.UtcNow.AddDays(-2)
        };
        var journeyState2 = new JourneyState
        {
            InstanceId = Guid.NewGuid().ToString(),
            Created = Clock.UtcNow.AddDays(-1).AddMinutes(Random.Shared.Next(0, 60)),
            State = "State2",
            UserId = Guid.NewGuid().ToString(),
            Updated = Clock.UtcNow.AddDays(-1).AddMinutes(Random.Shared.Next(0, 60))
        };
        var journeyState3 = new JourneyState
        {
            InstanceId = Guid.NewGuid().ToString(),
            Created = Clock.UtcNow.AddMinutes(Random.Shared.Next(-60, 0)),
            State = "State3",
            UserId = Guid.NewGuid().ToString(),
            Updated = Clock.UtcNow.AddMinutes(Random.Shared.Next(-60, 0))
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.JourneyStates.Add(journeyState1);
            dbContext.JourneyStates.Add(journeyState2);
            dbContext.JourneyStates.Add(journeyState3);
            await dbContext.SaveChangesAsync();
        });

        // Act
        await WithServiceAsync<DeleteStaleJourneyStatesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        var expected = new JourneyState[] { journeyState2, journeyState3 };
        var remainingJourneyStates = await WithDbContextAsync(dbContext => dbContext.JourneyStates.ToListAsync());
        Assert.All(remainingJourneyStates, s => Assert.Contains(s.InstanceId, expected.Select(e => e.InstanceId)));
        Assert.Equal(2, remainingJourneyStates.Count);
    }
}
