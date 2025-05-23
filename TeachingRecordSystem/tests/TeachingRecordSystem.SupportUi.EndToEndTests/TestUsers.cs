using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using AzAdUser = TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory.User;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class TestUsers
{
    public static User Administrator { get; } = new()
    {
        Active = true,
        Name = "Test administrator",
        Role = UserRoles.Administrator,
        UserId = Guid.NewGuid(),
        Email = "test.admin@localhost"
    };

    public static AzAdUser TestAzureActiveDirectoryUser { get; } = new()
    {
        UserId = Guid.NewGuid().ToString(),
        Email = Faker.Internet.Email(),
        Name = "Test AZ AD User"
    };

    public static AzAdUser TestLegacyAzureActiveDirectoryUser { get; } = new()
    {
        UserId = Guid.NewGuid().ToString(),
        Email = Faker.Internet.Email(),
        Name = "Test Legacy AZ AD User"
    };

    public class CreateUsersStartupTask(TrsDbContext trsDbContext) : IStartupTask
    {
        public Task ExecuteAsync()
        {
            trsDbContext.Users.Add(Administrator);

            return trsDbContext.SaveChangesAsync();
        }
    }
}
