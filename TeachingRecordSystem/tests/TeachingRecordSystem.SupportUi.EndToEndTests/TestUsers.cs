using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class TestUsers
{
    public static User Administrator { get; } = new()
    {
        Active = true,
        Name = "Test administrator",
        Roles = [UserRoles.Administrator],
        UserId = Guid.NewGuid(),
        Email = "test.admin@localhost"
    };

    public class CreateUsersStartupTask(TrsDbContext trsDbContext) : IStartupTask
    {
        public Task Execute()
        {
            trsDbContext.Users.Add(Administrator);

            return trsDbContext.SaveChangesAsync();
        }
    }
}
