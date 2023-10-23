using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetSystemUserByAzureActiveDirectoryObjectIdHandler : ICrmQueryHandler<GetSystemUserByAzureActiveDirectoryObjectIdQuery, SystemUser?>
{
    public async Task<SystemUser?> Execute(GetSystemUserByAzureActiveDirectoryObjectIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryByAttribute = new QueryByAttribute()
        {
            EntityName = SystemUser.EntityLogicalName,
            ColumnSet = query.ColumnSet
        };
        queryByAttribute.AddAttributeValue(SystemUser.Fields.AzureActiveDirectoryObjectId, query.AzureActiveDirectoryObjectId);

        var response = await organizationService.RetrieveMultipleAsync(queryByAttribute);

        return response.Entities.SingleOrDefault()?.ToEntity<SystemUser>();
    }
}
