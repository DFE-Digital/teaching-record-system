using System.Security.Claims;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class ClaimsExtensions
{
    extension(Core.DataStore.Postgres.Models.User user)
    {
        public IEnumerable<Claim> CreateClaims()
        {
            var claims = user.CreateRoleClaims()
                .Concat(user.CreatePermissionClaims())
                .Append(new Claim(CustomClaims.UserId, user.UserId.ToString()))
                .Append(new Claim(ClaimTypes.Name, user.Name));

            return claims;
        }

        public IEnumerable<Claim> CreateRoleClaims()
        {
            return user.Role != null ? [new Claim(ClaimTypes.Role, user.Role)] : [];
        }

        public IEnumerable<Claim> CreatePermissionClaims()
        {
            return UserRoles.GetPermissionsForRole(user.Role)
                .Select(permission => new Claim(CustomClaims.Permission, permission.ToString()));
        }
    }

    extension(ClaimsPrincipal user)
    {
        public bool HasMinimumPermission(UserPermission minimum)
        {
            var userPermissionLevel = user.FindAll(CustomClaims.Permission)
                .SelectMany(c => UserPermission.TryParse(c.Value, out var permission) ? [permission] : Array.Empty<UserPermission>())
                .Where(p => p.Type == minimum.Type)
                .Select(p => p.Level)
                .Append(UserPermissionLevel.None)
                .Max();

            return userPermissionLevel >= minimum.Level;
        }

        public bool HasBeenMigrated() =>
            user.FindAll(CustomClaims.Permission).Any();
    }

    // If user has been migrated to the new user roles, they will have a set of permission claims based on their role
}
