using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllIttProvidersHandler : ICrmQueryHandler<GetAllIttProvidersQuery, Account[]>
{
    public async Task<Account[]> ExecuteAsync(GetAllIttProvidersQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Account.Fields.dfeta_TrainingProvider, ConditionOperator.Equal, true);

        var queryExpression = new QueryExpression(Account.EntityLogicalName)
        {
            ColumnSet = new(
                Account.Fields.Name,
                Account.Fields.dfeta_UKPRN),
            Criteria = filter,
            Orders =
            {
                new OrderExpression(Account.Fields.Name, OrderType.Ascending)
            }
        };

        var request = new RetrieveMultipleRequest
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);
        return response.Entities.Select(x => x.ToEntity<Account>()).ToArray();
    }
}
