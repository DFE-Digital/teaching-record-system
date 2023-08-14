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

            return _dbContext.SaveChangesAsync();
        }
    }
}
