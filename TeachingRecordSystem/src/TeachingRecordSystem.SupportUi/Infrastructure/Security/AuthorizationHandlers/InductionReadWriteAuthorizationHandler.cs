using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class InductionReadWriteAuthorizationHandler : AuthorizationHandler<InductionReadWriteRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InductionReadWriteRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
