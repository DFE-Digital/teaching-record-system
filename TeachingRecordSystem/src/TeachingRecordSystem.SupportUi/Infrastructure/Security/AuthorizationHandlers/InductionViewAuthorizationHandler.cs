using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class InductionViewAuthorizationHandler : AuthorizationHandler<InductionViewRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InductionViewRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.View)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
