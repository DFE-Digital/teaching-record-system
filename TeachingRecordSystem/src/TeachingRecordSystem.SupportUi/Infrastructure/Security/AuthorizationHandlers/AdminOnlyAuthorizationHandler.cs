using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class AdminOnlyAuthorizationHandler : AuthorizationHandler<AdminOnlyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOnlyRequirement requirement)
    {
        // If the user has not yet been migrated to the new user roles, they may still have the Administrator role as
        // one of their legacy roles which hasn't been deleted yet.
        if (!context.User.HasBeenMigrated())
        {
            return Task.CompletedTask;
         }

        if (context.User.IsInRole(UserRoles.Administrator))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
