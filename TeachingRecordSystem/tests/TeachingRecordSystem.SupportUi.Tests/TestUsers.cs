using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests;

public class TestUsers(IDbContextFactory<TrsDbContext> dbContextFactory)
{
    private readonly Dictionary<string, User> _users = new();

    public User GetUser(string? role = null)
    {
        var userKey = role ?? "";

        // Note this method is synchronous so that it can be called from test class constructors
        lock (_users)
        {
            if (_users.TryGetValue(userKey, out var user))
            {
                return user;
            }

            using var dbContext = dbContextFactory.CreateDbContext();

            var userNumber = _users.Count + 1;

            user = new User()
            {
                Active = true,
                Email = $"test.user.{userNumber}@localhost",
                Name = $"Test User {userNumber}",
                Role = role,
                UserId = Guid.NewGuid()
            };

            dbContext.Users.Add(user);

            dbContext.SaveChanges();

            _users.Add(userKey, user);
            return user;
        }
    }

    public void ClearCache()
    {
        _users.Clear();
    }
}
