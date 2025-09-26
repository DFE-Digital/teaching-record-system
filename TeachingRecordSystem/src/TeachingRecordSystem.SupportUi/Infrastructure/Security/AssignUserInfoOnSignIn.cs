using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public class AssignUserInfoOnSignIn(string name) : IConfigureNamedOptions<OpenIdConnectOptions>
{
    private readonly string _name = name;

    public void Configure(string? name, OpenIdConnectOptions options)
    {
        if (_name != name)
        {
            return;
        }

        options.Scope.Add("email");

        options.Events.OnTicketReceived = async ctx =>
        {
            var aadUserId = ctx.Principal!.FindFirstValue("uid") ?? throw new Exception("Missing uid claim.");
            var email = ctx.Principal!.FindFirstValue(ClaimTypes.Email) ?? throw new Exception("Missing email address claim.");

            using var dbContext = ctx.HttpContext.RequestServices.GetRequiredService<TrsDbContext>();

            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.AzureAdUserId == aadUserId);

            if (user is null)
            {
                // We couldn't find a user by principal, but we may find them via email
                // (the CLI command to add a user creates a record *without* the AD subject).

                user = await dbContext.Users.SingleOrDefaultAsync(u =>
                    u.Email != null &&
                    EF.Functions.Collate(u.Email, Collations.CaseInsensitive) == email &&
                    u.Active &&
                    u.AzureAdUserId == null);

                if (user is not null)
                {
                    user.AzureAdUserId = aadUserId;
                    await dbContext.SaveChangesAsync();
                }
            }

            if (user is null)
            {
                return;
            }

            await SyncUserInfoAsync();

            var identityWithRoles = new ClaimsIdentity(
                ctx.Principal!.Identity,
                user.CreateClaims(),
                authenticationType: ctx.Principal.Identity!.AuthenticationType,
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            ctx.Principal = new ClaimsPrincipal(identityWithRoles);

            async Task SyncUserInfoAsync()
            {
                // The GraphServiceClient used within AadUserService needs HttpContext.User assigned so it can retrieve the access token;
                // temporarily assign HttpContext.User for this service call then reset it when we're done.

                var oldPrincipal = ctx.HttpContext.User;
                ctx.HttpContext.User = ctx.Principal!;

                try
                {
                    var aadUserService = ctx.HttpContext.RequestServices.GetRequiredService<IAadUserService>();
                    var azureAdUser = (await aadUserService.GetUserByIdAsync(aadUserId))!;
                    user.Email = azureAdUser.Email;
                    user.Name = azureAdUser.Name;
                    await dbContext.SaveChangesAsync();
                }
                finally
                {
                    ctx.HttpContext.User = oldPrincipal;
                }
            }
        };
    }

    public void Configure(OpenIdConnectOptions options)
    {
    }
}
