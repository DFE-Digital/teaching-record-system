using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class FindActiveOrganisationsByLaSchoolCodeHandler : ICrmQueryHandler<FindActiveOrganisationsByLaSchoolCodeQuery, Account[]>
{
    public async Task<Account[]> ExecuteAsync(FindActiveOrganisationsByLaSchoolCodeQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = Account.EntityLogicalName,
            ColumnSet = new ColumnSet(
                Account.Fields.AccountNumber,
                Account.Fields.dfeta_LASchoolCode,
                Account.Fields.Name)
        };

        queryExpression.Criteria.AddCondition(Account.Fields.dfeta_LASchoolCode, ConditionOperator.Equal, query.LaSchoolCode);
        queryExpression.Criteria.AddCondition(Account.Fields.StateCode, ConditionOperator.Equal, (int)AccountState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<Account>()).ToArray();
    }
}
