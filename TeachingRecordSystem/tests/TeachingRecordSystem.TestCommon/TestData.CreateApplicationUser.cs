using System.Security.Cryptography;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<ApplicationUser> CreateApplicationUser(
        string? name = null,
        string[]? apiRoles = null,
        bool? hasOneLoginSettings = false)
    {
        var user = await WithDbContext(async dbContext =>
        {
            name ??= GenerateApplicationUserName();
            apiRoles ??= [];
            string? oneLoginClientId = null;
            string? oneLoginPrivateKeyPem = null;
            if (hasOneLoginSettings == true)
            {
                oneLoginClientId = Guid.NewGuid().ToString();
                oneLoginPrivateKeyPem = GeneratePrivateKeyPem();
            }

            var user = new ApplicationUser()
            {
                Name = name,
                UserId = Guid.NewGuid(),
                ApiRoles = apiRoles,
                OneLoginClientId = oneLoginClientId,
                OneLoginPrivateKeyPem = oneLoginPrivateKeyPem
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

    public static string GeneratePrivateKeyPem()
    {
        return RSA.Create().ExportRSAPrivateKeyPem();
    }
}
