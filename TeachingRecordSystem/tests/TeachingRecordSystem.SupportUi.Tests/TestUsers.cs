using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests;

public static class TestUsers
{
    public static User Administrator { get; } = new()
    {
        Active = true,
        Name = "Test administrator",
        Roles = [UserRoles.Administrator],
        UserId = Guid.NewGuid(),
        Email = "test.administrator@localhost"
    };

    public static User Helpdesk { get; } = new()
    {
        Active = true,
        Name = "Test helpdesk user",
        Roles = [UserRoles.Helpdesk],
        UserId = Guid.NewGuid(),
        Email = "test.helpdesk@localhost"
    };

    public static User UnusedRole { get; } = new()
    {
        Active = true,
        Name = "Test other user",
        Roles = ["UnusedRole"],
        UserId = Guid.NewGuid(),
        Email = "test.other@localhost"
    };

    public static User NoRoles { get; } = new()
    {
        Active = true,
        Name = "No roles",
        Roles = [],
        UserId = Guid.NewGuid(),
        Email = "test.empty@localhost"
    };

    public class CreateUsersStartupTask(TrsDbContext trsDbContext) : IStartupTask
    {
        public Task Execute()
        {
            trsDbContext.Users.Add(Administrator);
            trsDbContext.Users.Add(Helpdesk);
            trsDbContext.Users.Add(UnusedRole);
            trsDbContext.Users.Add(NoRoles);

            return trsDbContext.SaveChangesAsync();
        }
    }
}
