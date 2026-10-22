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
                Team = EventModels.Team.FromModel(team)
            },
            processContext,
            cancellationToken);

        return team;
    }

    public async Task DeleteTeamAsync(Team team, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        var members = await dbContext.Users.Where(u => u.TeamId == team.TeamId).ToArrayAsync(cancellationToken);
        foreach (var member in members)
        {
            member.TeamId = null;
        }

        team.DeletedOn = processContext.Now;
        await dbContext.SaveChangesAsync(cancellationToken);

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);
        await eventScope.PublishEventAsync(
            new TeamDeletedEvent
            {
                EventId = Guid.NewGuid(),
                Team = EventModels.Team.FromModel(team)
            },
            cancellationToken);

        foreach (var member in members)
        {
            await eventScope.PublishEventAsync(
                new UserUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    User = EventModels.User.FromModel(member),
                    Changes = UserUpdatedEventChanges.Team
                },
                cancellationToken);
        }
    }
}
