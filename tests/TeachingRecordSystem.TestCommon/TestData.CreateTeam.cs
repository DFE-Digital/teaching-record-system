using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<Team> CreateTeamAsync(string? name = null) =>
        WithDbContextAsync(async dbContext =>
        {
            var team = new Team()
            {
                TeamId = Guid.NewGuid(),
                Name = name ?? $"Team {Guid.NewGuid():N}",
                CreatedOn = TimeProvider.UtcNow
            };

            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();

            return team;
        });
}
