using System.Security.Claims;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class ClaimsExtensions
{
    public static IEnumerable<Claim> CreateClaims(this Core.DataStore.Postgres.Models.User user)
    {
        var claims = user.CreateLegacyRoleClaims()
            .Concat(user.CreateRoleClaims())
            .Concat(user.CreatePermissionClaims())
            .Append(new Claim(CustomClaims.UserId, user.UserId.ToString()))
            .Append(new Claim(ClaimTypes.Name, user.Name));

        if (user.DqtUserId is Guid dqtUserId)
        {
            claims = claims.Append(new Claim(CustomClaims.DqtUserId, dqtUserId.ToString()));
        }

        return claims;
    }

    public static IEnumerable<Claim> CreateLegacyRoleClaims(this Core.DataStore.Postgres.Models.User user)
    {
        return user.Roles?.Select(r => new Claim(ClaimTypes.Role, r)) ?? [];
    }

    public static IEnumerable<Claim> CreateRoleClaims(this Core.DataStore.Postgres.Models.User user)
    {
        return user.Role != null ? [new Claim(ClaimTypes.Role, user.Role)] : [];
    }

    public static IEnumerable<Claim> CreatePermissionClaims(this Core.DataStore.Postgres.Models.User user)
    {
        return UserRoles.GetPermissionsForRole(user.Role)
            .Select(permission => new Claim(CustomClaims.Permission, permission.ToString()));
    }

    public static bool HasMinimumPermission(this ClaimsPrincipal user, UserPermission minimum)
    {
        var userPermissionLevel = user.FindAll(CustomClaims.Permission)
            .SelectMany(c => UserPermission.TryParse(c.Value, out var permission) ? [permission] : new UserPermission[0])
            .Where(p => p.Type == minimum.Type)
            .Select(p => p.Level)
            .Append(UserPermissionLevel.None)
            .Max();

        return userPermissionLevel >= minimum.Level;
    }
}
