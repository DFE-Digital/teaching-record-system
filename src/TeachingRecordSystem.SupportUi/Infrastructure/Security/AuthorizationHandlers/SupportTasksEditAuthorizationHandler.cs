using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class SupportTasksEditAuthorizationHandler : AuthorizationHandler<SupportTasksEditRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SupportTasksEditRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.SupportTasks, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
