using System.Data;
using System;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Legacy;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<User> CreateUserAsync(
        bool? active = null,
        string? name = null,
        string? email = null,
        string? role = null,
        Guid? azureAdUserId = null)
    {
        var user = await WithDbContextAsync(async dbContext =>
        {
            active ??= true;
            name ??= GenerateName();
            email ??= GenerateUniqueEmail();
            role ??= UserRoles.SupportOfficer;

            var user = new User()
            {
                Active = active.Value,
                Name = name,
                Email = email,
                Role = role,
                UserId = Guid.NewGuid(),
                AzureAdUserId = azureAdUserId?.ToString(),
            };

            dbContext.Users.Add(user);

            await dbContext.SaveChangesAsync();

            return user;
        });

        return user;
    }


    public async Task<IReadOnlyCollection<User>> CreateMultipleUsersAsync(
        int userCount,
        Func<int, CreateUserSpec> createUserSpecFromIndex)
    {
        var users = await WithDbContextAsync(async dbContext =>
        {
            for (var i = 0; i < userCount; i++)
            {
                var spec = createUserSpecFromIndex(i);
                var active = spec.Active ?? true;
                var name = spec.Name ?? GenerateName();
                var email = spec.Email ?? GenerateUniqueEmail();
                var role = spec.Role ?? UserRoles.SupportOfficer;

                var user = new User()
                {
                    Active = active,
                    Name = name,
                    Email = email,
                    Role = role,
                    UserId = Guid.NewGuid()
                };

                dbContext.Users.Add(user);
            }

            await dbContext.SaveChangesAsync();

            return dbContext.Users.AsReadOnly();
        });

        return users;
    }

    public async Task<User> CreateUserWithLegacyRolesAsync(
        bool? active = null,
        string? name = null,
        string? email = null,
        string[]? roles = null,
        Guid? azureAdUserId = null)
    {
        var user = await WithDbContextAsync(async dbContext =>
        {
            active ??= true;
            name ??= GenerateName();
            email ??= GenerateUniqueEmail();
            roles ??= [LegacyUserRoles.Helpdesk];

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

    public async Task CreateCrmUserAsync(
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

        await txnRequestBuilder.ExecuteAsync();
    }

    public class CreateUserSpec
    {
        public bool? Active { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
