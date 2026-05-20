using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class SupportTasksViewAuthorizationHandler : AuthorizationHandler<SupportTasksViewRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SupportTasksViewRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.SupportTasks, UserPermissionLevel.View)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
