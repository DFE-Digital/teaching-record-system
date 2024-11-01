using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class FindOrganisationsByOrgNumberHandler : ICrmQueryHandler<FindOrganisationsByOrgNumberQuery, Account[]>
{
    public async Task<Account[]> Execute(FindOrganisationsByOrgNumberQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = Account.EntityLogicalName,
            ColumnSet = new ColumnSet(
                Account.Fields.AccountNumber,
                Account.Fields.Name)
        };

        queryExpression.Criteria.AddCondition(Account.Fields.AccountNumber, ConditionOperator.Equal, query.OrganisationNumber);
        queryExpression.Criteria.AddCondition(Account.Fields.StateCode, ConditionOperator.Equal, (int)AccountState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<Account>()).ToArray();
    }
}
