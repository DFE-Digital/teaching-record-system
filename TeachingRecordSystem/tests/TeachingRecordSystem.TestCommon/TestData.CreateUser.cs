using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<User> CreateUserAsync(
        bool? active = null,
        string? name = null,
        string? email = null,
        string? role = null,
        Guid? azureAdUserId = null)
    {
        var user = await WithDbContextAsync(async dbContext =>
        {
            active ??= true;
            name ??= GenerateName();
            email ??= GenerateUniqueEmail();
            role ??= UserRoles.RecordManager;

            var user = new User()
            {
                Active = active.Value,
                Name = name,
                Email = email,
                Role = role,
                UserId = Guid.NewGuid(),
                AzureAdUserId = azureAdUserId?.ToString()
            };

            dbContext.Users.Add(user);

            await dbContext.SaveChangesAsync();

            return user;
        });

        return user;
    }

    public async Task<IReadOnlyList<User>> CreateMultipleUsersAsync(
        int userCount,
        Func<int, CreateUserSpec> createUserSpecFromIndex)
    {
        var users = await WithDbContextAsync(async dbContext =>
        {
            for (var i = 0; i < userCount; i++)
            {
                var spec = createUserSpecFromIndex(i);
                var active = spec.Active ?? true;
                var name = spec.Name ?? GenerateName();
                var email = spec.Email ?? GenerateUniqueEmail();
                var role = spec.Role ?? UserRoles.RecordManager;

                var user = new User()
                {
                    Active = active,
                    Name = name,
                    Email = email,
                    Role = role,
                    UserId = Guid.NewGuid()
                };

                dbContext.Users.Add(user);
            }

            await dbContext.SaveChangesAsync();

            return dbContext.Users.ToList();
        });

        return users;
    }

    public async Task<IReadOnlyList<User>> CreateMultipleUsersAsync(params CreateUserSpec[] userSpecs)
    {
        var users = await WithDbContextAsync(async dbContext =>
        {
            foreach (var spec in userSpecs)
            {
                var active = spec.Active ?? true;
                var name = spec.Name ?? GenerateName();
                var email = spec.Email ?? GenerateUniqueEmail();
                var role = spec.Role ?? UserRoles.RecordManager;

                var user = new User()
                {
                    Active = active,
                    Name = name,
                    Email = email,
                    Role = role,
                    UserId = Guid.NewGuid()
                };

                dbContext.Users.Add(user);
            }

            await dbContext.SaveChangesAsync();

            return dbContext.Users.ToList();
        });

        return users;
    }

    public class CreateUserSpec
    {
        public bool? Active { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
