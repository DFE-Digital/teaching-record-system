using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class InductionEditAuthorizationHandler : AuthorizationHandler<InductionEditRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InductionEditRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
