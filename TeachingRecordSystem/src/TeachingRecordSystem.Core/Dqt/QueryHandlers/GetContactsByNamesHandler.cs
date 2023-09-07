using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetContactsByNamesHandler : ICrmQueryHandler<GetContactsByNamesQuery, Contact[]>
{
    public async Task<Contact[]> Execute(GetContactsByNamesQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        foreach (var name in query.Names)
        {
            filter.AddCondition(Contact.Fields.FullName, ConditionOperator.Contains, name);
        }     

        var queryExpression = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = query.ColumnSet,
            Criteria = filter
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
    }
}
