using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests;

public class TestUsers(IDbContextFactory<TrsDbContext> dbContextFactory)
{
    private static int _userCount;

    public User CreateUser(string? role = null)
    {
        // Note this method is synchronous so that it can be called from test class constructors
        using var dbContext = dbContextFactory.CreateDbContext();

        var userNumber = Interlocked.Increment(ref _userCount);

        var user = new User
        {
            Active = true,
            Email = $"test.user.{userNumber}@localhost",
            Name = $"Test User {userNumber}",
            Role = role,
            UserId = Guid.NewGuid()
        };

        dbContext.Users.Add(user);

        dbContext.SaveChanges();

        return user;
    }
}
