using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyChangeManagementAuthorizationHandler : AuthorizationHandler<ChangeManagementRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ChangeManagementRequirement requirement)
    {
        if (context.User.IsInRole(LegacyUserRoles.Helpdesk)
            || context.User.IsInRole(LegacyUserRoles.Administrator))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
