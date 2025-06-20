using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class RoutesEditAuthorizationHandler : AuthorizationHandler<RoutesEditRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoutesEditRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.Routes, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
