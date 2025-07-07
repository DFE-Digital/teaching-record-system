using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class NonPersonOrAlertDataAuthorization
{
    public static AuthorizationBuilder AddNonPersonOrAlertDataPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.NonPersonOrAlertDataView,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new NonPersonOrAlertDataViewRequirement()));

        builder.AddPolicy(
            AuthorizationPolicies.NonPersonOrAlertDataEdit,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new NonPersonOrAlertDataEditRequirement()));

        builder.AddPolicy(
            AuthorizationPolicies.NotesView,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new NotesViewRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, NonPersonOrAlertDataViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, NonPersonOrAlertDataEditAuthorizationHandler>()
            // NotesViewAuthorizationHandler consumes TrsDbContext which is scoped
            .AddScoped<IAuthorizationHandler, NotesViewAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyNonPersonOrAlertDataViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyNonPersonOrAlertDataEditAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyNotesViewAuthorizationHandler>();


        return builder;
    }
}
