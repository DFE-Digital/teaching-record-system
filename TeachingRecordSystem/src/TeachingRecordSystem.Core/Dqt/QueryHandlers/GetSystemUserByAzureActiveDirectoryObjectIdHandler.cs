using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetSystemUserByAzureActiveDirectoryObjectIdHandler : ICrmQueryHandler<GetSystemUserByAzureActiveDirectoryObjectIdQuery, SystemUserInfo?>
{
    public async Task<SystemUserInfo?> Execute(GetSystemUserByAzureActiveDirectoryObjectIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(SystemUser.Fields.AzureActiveDirectoryObjectId, ConditionOperator.Equal, query.AzureActiveDirectoryObjectId);

        var queryExpression = new QueryExpression(SystemUser.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                SystemUser.PrimaryIdAttribute,
                SystemUser.Fields.IsDisabled),
            Criteria = filter
        };

        var systemRolesLink = queryExpression.AddLink(
            SystemUserRoles.EntityLogicalName,
            SystemUser.PrimaryIdAttribute,
            SystemUserRoles.Fields.SystemUserId,
            JoinOperator.LeftOuter);

        systemRolesLink.Columns = new ColumnSet(
            SystemUserRoles.PrimaryIdAttribute,
            SystemUserRoles.Fields.RoleId);

        systemRolesLink.EntityAlias = SystemUserRoles.EntityLogicalName;

        var rolesLink = systemRolesLink.AddLink(
            Role.EntityLogicalName,
            SystemUserRoles.Fields.RoleId,
            Role.PrimaryIdAttribute,
            JoinOperator.LeftOuter);

        rolesLink.Columns = new ColumnSet(
            Role.PrimaryIdAttribute,
            Role.Fields.Name);

        rolesLink.EntityAlias = $"{SystemUserRoles.EntityLogicalName}.{Role.EntityLogicalName}_entity";

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        var systemUsersAndRoles = response.Entities.Select(e => e.ToEntity<SystemUser>())
            .Select(su =>
                (SystemUser: su,
                Role: (su.Extract<SystemUserRoles>(SystemUserRoles.EntityLogicalName, SystemUserRoles.PrimaryIdAttribute)?.Extract<Role>($"{Role.EntityLogicalName}_entity", Role.PrimaryIdAttribute))));

        var returnValue = systemUsersAndRoles
            .GroupBy(t => t.SystemUser.Id)
            .Select(g => (g.First().SystemUser, Roles: g.Where(i => i.Role != null).Select(i => i.Role!)))
            .Select(i => new SystemUserInfo(i.SystemUser, i.Roles.ToArray()))
            .FirstOrDefault();

        return returnValue;
    }
}
