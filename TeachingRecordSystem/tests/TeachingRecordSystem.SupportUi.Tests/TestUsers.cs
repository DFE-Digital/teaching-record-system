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

    public static User Helpdesk { get; } = new()
    {
        Active = true,
        Name = "Test helpdesk user",
        Roles = new[] { UserRoles.Helpdesk },
        UserId = Guid.NewGuid(),
        UserType = UserType.Person
    };

    public static User UnusedRole { get; } = new()
    {
        Active = true,
        Name = "Test other user",
        Roles = new[] { "UnusedRole" },
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
            _dbContext.Users.Add(Helpdesk);
            _dbContext.Users.Add(UnusedRole);
            _dbContext.Users.Add(NoRoles);

            return _dbContext.SaveChangesAsync();
        }
    }
}
