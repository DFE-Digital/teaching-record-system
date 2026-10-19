using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Teams;

public class TeamService(TrsDbContext dbContext, IEventPublisher eventPublisher)
{
    public async Task<Team> CreateTeamAsync(string name, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        var team = new Team
        {
            TeamId = Guid.NewGuid(),
            Name = name,
            CreatedOn = processContext.Now
        };

        dbContext.Teams.Add(team);
        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishSingleEventAsync(
            new TeamCreatedEvent
            {
                EventId = Guid.NewGuid(),
                TeamId = team.TeamId,
                Name = team.Name
            },
            processContext,
            cancellationToken);

        return team;
    }
}
