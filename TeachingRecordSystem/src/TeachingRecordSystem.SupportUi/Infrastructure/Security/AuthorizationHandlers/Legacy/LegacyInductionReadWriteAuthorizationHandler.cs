using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyInductionReadWriteAuthorizationHandler : AuthorizationHandler<InductionReadWriteRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InductionReadWriteRequirement requirement)
    {
        if (context.User.IsInRole(LegacyUserRoles.InductionReadWrite)
            || context.User.IsInRole(LegacyUserRoles.Administrator))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
