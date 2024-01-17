using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<ApplicationUser> CreateApplicationUser(
        string? name = null,
        string[]? apiRoles = null)
    {
        var user = await WithDbContext(async dbContext =>
        {
            name ??= GenerateApplicationUserName();
            apiRoles ??= [];

            var user = new ApplicationUser()
            {
                Name = name,
                UserId = Guid.NewGuid(),
                ApiRoles = apiRoles
            };

            dbContext.ApplicationUsers.Add(user);

            var @event = new ApplicationUserCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                RaisedBy = SystemUser.SystemUserId,
                CreatedUtc = Clock.UtcNow,
                ApplicationUser = Core.Events.Models.ApplicationUser.FromModel(user)
            };
            dbContext.AddEvent(@event);

            await dbContext.SaveChangesAsync();

            return user;
        });

        return user;
    }
}
