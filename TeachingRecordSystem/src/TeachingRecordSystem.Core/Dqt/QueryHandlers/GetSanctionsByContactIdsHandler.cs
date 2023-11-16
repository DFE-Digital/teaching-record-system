using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetSanctionsByContactIdsHandler : ICrmQueryHandler<GetSanctionsByContactIdsQuery, IDictionary<Guid, SanctionResult[]>>
{
    public async Task<IDictionary<Guid, SanctionResult[]>> Execute(
        GetSanctionsByContactIdsQuery query,
        IOrganizationServiceAsync organizationService)
    {
        var contactIdsArray = query.ContactIds.ToArray();

        if (contactIdsArray.Length == 0)
        {
            return new Dictionary<Guid, SanctionResult[]>();
        }

        var sanctionColumns = new ColumnSet(dfeta_sanction.Fields.dfeta_PersonId);
        sanctionColumns.AddColumns(query.ColumnSet.Columns.ToArray());

        var queryExpression = new QueryExpression(dfeta_sanction.EntityLogicalName)
        {
            ColumnSet = sanctionColumns
        };

        if (query.ActiveOnly)
        {
            queryExpression.Criteria.AddCondition(dfeta_sanction.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_sanctionState.Active);
        }

        queryExpression.Criteria.AddCondition(dfeta_sanction.Fields.dfeta_PersonId, ConditionOperator.In, contactIdsArray.Cast<object>().ToArray());  // https://community.dynamics.com/crm/b/crmbusiness/posts/crm-2011-odd-error-with-query-expression-and-conditionoperator-in

        var sanctionCodeLink = queryExpression.AddLink(
            dfeta_sanctioncode.EntityLogicalName,
            dfeta_sanction.Fields.dfeta_SanctionCodeId,
            dfeta_sanctioncode.PrimaryIdAttribute,
            JoinOperator.Inner);

        sanctionCodeLink.Columns = new ColumnSet(dfeta_sanctioncode.PrimaryIdAttribute, dfeta_sanctioncode.Fields.dfeta_Value);
        sanctionCodeLink.EntityAlias = typeof(dfeta_sanctioncode).Name;

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var result = (RetrieveMultipleResponse)await organizationService.ExecuteAsync(request);

        var sanctionsByContactIds = result.EntityCollection.Entities
            .GroupBy(r => r.GetAttributeValue<EntityReference>(dfeta_sanction.Fields.dfeta_PersonId).Id)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(e => new SanctionResult(e.ToEntity<dfeta_sanction>(), e.Extract<dfeta_sanctioncode>().dfeta_Value))
                    .OrderBy(t => t.SanctionCode)  // Ensure we always return sanction codes in the same order
                    .ToArray());

        return contactIdsArray.ToDictionary(id => id, id => sanctionsByContactIds.GetValueOrDefault(id, Array.Empty<SanctionResult>()));
    }
}
