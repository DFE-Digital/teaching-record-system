using System.Diagnostics.CodeAnalysis;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests;

public class TestUsers(IDbContextFactory<TrsDbContext> dbContextFactory)
{
    private readonly Dictionary<HashSet<string>, User> _users = new(new RoleSetEqualityComparer());

    public User GetUser(string? role) => GetUser(role is not null ? [role] : []);

    public User GetUser(params string[] roles) => GetUser(roles.AsEnumerable());

    public User GetUser(IEnumerable<string> roles)
    {
        // Note this method is synchronous so that it can be called from test class constructors

        var rolesSet = new HashSet<string>(roles);

        lock (_users)
        {
            if (_users.TryGetValue(rolesSet, out var user))
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
                Roles = roles.ToArray(),
                UserId = Guid.NewGuid()
            };

            dbContext.Users.Add(user);

            dbContext.SaveChanges();

            _users.Add(rolesSet, user);
            return user;
        }
    }

    private class RoleSetEqualityComparer : IEqualityComparer<HashSet<string>>
    {
        public bool Equals(HashSet<string>? x, HashSet<string>? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.SetEquals(y);
        }

        public int GetHashCode([DisallowNull] HashSet<string> obj)
        {
            var hashCode = new HashCode();
            foreach (var entry in obj)
            {
                hashCode.Add(entry.GetHashCode());
            }
            return hashCode.ToHashCode();
        }
    }
}
