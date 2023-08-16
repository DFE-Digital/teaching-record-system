using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<User> CreateUser(
        string? name = null,
        string? email = null,
        string[]? roles = null)
    {
        return WithDbContext(async dbContext =>
        {
            name ??= GenerateName();
            email ??= GenerateUniqueEmail();
            roles ??= new[] { UserRoles.Helpdesk };

            var user = new User()
            {
                Active = true,
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
