using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyRoutesEditAuthorizationHandler : AuthorizationHandler<RoutesEditRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoutesEditRequirement requirement)
    {
        // If the user has been migrated to the new user roles, they may still have the legacy roles, so we need to
        // disregard them for this user as they may be different to their new role.
        if (context.User.HasBeenMigrated())
        {
            return Task.CompletedTask;
        }

        // Qualifications were previously editable by all users, so if the user hasn't been migrated yet,
        // they should still be able to edit qualifications.
        // Note: This implies they will also be able to edit Routes, as Routes and Qualifications are covered by the
        // same set of permissions, but this shouldn't be an issue as the Routes functionality will only be enabled
        // once all users have been migrated to the new user roles.
        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
