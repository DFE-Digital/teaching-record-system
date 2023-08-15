using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public class AssignUserInfoOnSignIn : IConfigureNamedOptions<OpenIdConnectOptions>
{
    private readonly string _name;

    public AssignUserInfoOnSignIn(string name)
    {
        _name = name;
    }

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
                // (the CLI commmand to add a user creates a record *without* the AD subject).

                user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == email && u.Active == true && u.AzureAdUserId == null);

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

            if (user.Email?.Equals(email, StringComparison.OrdinalIgnoreCase) != true)
            {
                user.Email = email;
                await dbContext.SaveChangesAsync();
            }

            var claims = user.Roles.Select(r => new Claim(ClaimTypes.Role, r))
                .Append(new Claim(CustomClaims.UserId, user.UserId.ToString()))
                .Append(new Claim(ClaimTypes.Name, user.Name));

            var crmUserId = await GetCrmUserId();
            if (crmUserId.HasValue)
            {
                claims = claims.Append(new Claim(CustomClaims.CrmUserId, crmUserId.Value.ToString()));
            }

            var identityWithRoles = new ClaimsIdentity(
                ctx.Principal!.Identity,
                claims,
                authenticationType: ctx.Principal.Identity!.AuthenticationType,
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            ctx.Principal = new ClaimsPrincipal(identityWithRoles);

            async Task<Guid?> GetCrmUserId()
            {
                using var serviceClient = ctx.HttpContext.RequestServices.GetRequiredService<ServiceClient>();

                var request = new QueryByAttribute(SystemUser.EntityLogicalName);
                request.AddAttributeValue(SystemUser.Fields.AzureActiveDirectoryObjectId, new Guid(aadUserId));

                var response = await serviceClient.RetrieveMultipleAsync(request);

                if (response.Entities.Count == 0)
                {
                    return null;
                }

                return response.Entities.Single().Id;
            }
        };
    }

    public void Configure(OpenIdConnectOptions options)
    {
    }
}
