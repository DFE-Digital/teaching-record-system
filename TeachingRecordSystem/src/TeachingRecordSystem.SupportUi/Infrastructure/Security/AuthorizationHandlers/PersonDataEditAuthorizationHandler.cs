using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class PersonDataEditAuthorizationHandler : AuthorizationHandler<PersonDataEditRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PersonDataEditRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.PersonData, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
