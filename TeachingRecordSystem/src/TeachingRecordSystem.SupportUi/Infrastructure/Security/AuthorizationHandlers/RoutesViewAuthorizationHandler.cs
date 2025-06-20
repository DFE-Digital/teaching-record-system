using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class RoutesViewAuthorizationHandler : AuthorizationHandler<RoutesViewRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoutesViewRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.Routes, UserPermissionLevel.View)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
