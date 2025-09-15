using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class SupportTasksAuthorization
{
    public static AuthorizationBuilder AddSupportTasksPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.SupportTasksView,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new SupportTasksViewRequirement()));

        builder.AddPolicy(
            AuthorizationPolicies.SupportTasksEdit,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new SupportTasksEditRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, SupportTasksViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, SupportTasksEditAuthorizationHandler>();

        return builder;
    }
}
