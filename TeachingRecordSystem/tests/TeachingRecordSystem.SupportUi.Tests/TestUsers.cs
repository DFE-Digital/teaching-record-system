using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests;

public static class TestUsers
{
    public static User Administrator { get; } = new()
    {
        Active = true,
        Name = "Test administrator",
        Roles = new[] { UserRoles.Administrator },
        UserId = Guid.NewGuid(),
        UserType = UserType.Person
    };

    public static User NoRoles { get; } = new()
    {
        Active = true,
        Name = "No roles",
        Roles = Array.Empty<string>(),
        UserId = Guid.NewGuid(),
        UserType = UserType.Person
    };

    public static User CreateUser(bool active = true, UserType userType = UserType.Person, string? name = null, string? email = null, string? azureId = null, string[]? roles = null)
    {
        return new User
        {
            UserId = Guid.NewGuid(),
            AzureAdUserId = azureId ?? Guid.NewGuid().ToString("D"),
            Active = active,
            UserType = userType,
            Name = name ?? Faker.Name.FullName(),
            Email = email ?? Faker.Internet.Email(),
            Roles = roles ?? new[] { UserRoles.Administrator }
        };
    }

    public class CreateUsersStartupTask : IStartupTask
    {
        private readonly TrsDbContext _dbContext;

        public CreateUsersStartupTask(TrsDbContext trsDbContext)
        {
            _dbContext = trsDbContext;
        }

        public Task Execute()
        {
            _dbContext.Users.Add(Administrator);
            _dbContext.Users.Add(NoRoles);

            return _dbContext.SaveChangesAsync();
        }
    }
}
