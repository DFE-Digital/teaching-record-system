using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<User> CreateUser(
        bool? active = null,
        string? name = null,
        string? email = null,
        string[]? roles = null)
    {
        return WithDbContext(async dbContext =>
        {
            active ??= true;
            name ??= GenerateName();
            email ??= GenerateUniqueEmail();
            roles ??= new[] { UserRoles.Helpdesk };

            var user = new User()
            {
                Active = active.Value,
                Name = name,
                Email = email,
                Roles = roles,
                UserId = Guid.NewGuid(),
                UserType = UserType.Person,
                AzureAdUserId = null
            };

            dbContext.Users.Add(user);

            await dbContext.SaveChangesAsync();

            return user;
        });
    }
}
