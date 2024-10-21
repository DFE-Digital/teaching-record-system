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

    public static User AllAlertsWriter { get; } = new()
    {
        Active = true,
        Name = "Test all alerts writer",
        Roles = [UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite],
        UserId = Guid.NewGuid(),
        Email = "test.alert-writer@localhost"
    };

    public static User AllAlertsReader { get; } = new()
    {
        Active = true,
        Name = "Test all alerts reader",
        Roles = [UserRoles.DbsAlertsReadOnly],
        UserId = Guid.NewGuid(),
        Email = "test.alert-reader@localhost"
    };

    public static User NonDbsAlertWriter { get; } = new()
    {
        Active = true,
        Name = "Test non-DBS alert writer",
        Roles = [UserRoles.AlertsReadWrite],
        UserId = Guid.NewGuid(),
        Email = "test.non-dbs-alert-writer@localhost"
    };

    public static User DbsAlertReader { get; } = new()
    {
        Active = true,
        Name = "Test DBS alert reader",
        Roles = [UserRoles.DbsAlertsReadOnly],
        UserId = Guid.NewGuid(),
        Email = "test.dbs-alert-reader@localhost"
    };

    public static User DbsAlertWriter { get; } = new()
    {
        Active = true,
        Name = "Test DBS alert reader",
        Roles = [UserRoles.DbsAlertsReadWrite],
        UserId = Guid.NewGuid(),
        Email = "test.dbs-alert-writer@localhost"
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
            trsDbContext.Users.Add(AllAlertsWriter);
            trsDbContext.Users.Add(AllAlertsReader);
            trsDbContext.Users.Add(NonDbsAlertWriter);
            trsDbContext.Users.Add(DbsAlertReader);
            trsDbContext.Users.Add(DbsAlertWriter);
            trsDbContext.Users.Add(NoRoles);

            return trsDbContext.SaveChangesAsync();
        }
    }
}
