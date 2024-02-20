using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<User> CreateUser(
        bool? active = null,
        string? name = null,
        string? email = null,
        string[]? roles = null,
        Guid? azureAdUserId = null)
    {
        var user = await WithDbContext(async dbContext =>
        {
            active ??= true;
            name ??= GenerateName();
            email ??= GenerateUniqueEmail();
            roles ??= [UserRoles.Helpdesk];

            var user = new User()
            {
                Active = active.Value,
                Name = name,
                Email = email,
                Roles = roles,
                UserId = Guid.NewGuid(),
                AzureAdUserId = azureAdUserId?.ToString(),
            };

            dbContext.Users.Add(user);

            await dbContext.SaveChangesAsync();

            return user;
        });

        return user;
    }

    public async Task CreateCrmUser(
        Guid azureAdUserId,
        bool hasDisabledCrmAccount = false,
        string[]? dqtRoles = null)
    {
        var txnRequestBuilder = RequestBuilder.CreateTransaction(this.OrganizationService);

        var systemUserId = Guid.NewGuid();
        txnRequestBuilder.AddRequest(new CreateRequest()
        {
            Target = new Core.Dqt.Models.SystemUser()
            {
                Id = systemUserId,
                AzureActiveDirectoryObjectId = azureAdUserId,
                IsDisabled = hasDisabledCrmAccount
            }
        });

        if (dqtRoles is not null)
        {
            foreach (var dqtRole in dqtRoles)
            {
                var roleId = Guid.NewGuid();
                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new Role()
                    {
                        RoleId = roleId,
                        Name = dqtRole
                    }
                });

                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new SystemUserRoles()
                    {
                        SystemUserId = systemUserId,
                        RoleId = roleId
                    }
                });
            }
        }

        await txnRequestBuilder.Execute();
    }
}
