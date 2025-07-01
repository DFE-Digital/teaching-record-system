using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class PersonDataAuthorization
{
    public static AuthorizationBuilder AddPersonDataPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.PersonDataEdit,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PersonDataEditRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, PersonDataEditAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyPersonDataEditAuthorizationHandler>();

        return builder;
    }
}
