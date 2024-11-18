using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetActiveQtsRegistrationsByContactIdsHandler : ICrmQueryHandler<GetActiveQtsRegistrationsByContactIdsQuery, IDictionary<Guid, dfeta_qtsregistration[]>>
{
    public async Task<IDictionary<Guid, dfeta_qtsregistration[]>> ExecuteAsync(GetActiveQtsRegistrationsByContactIdsQuery query, IOrganizationServiceAsync organizationService)
    {
        var contactIdsArray = query.ContactIds.ToArray();

        if (contactIdsArray.Length == 0)
        {
            return new Dictionary<Guid, dfeta_qtsregistration[]>();
        }

        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_qtsregistration.EntityLogicalName,
            ColumnSet = query.ColumnSet
        };
        queryExpression.Criteria.AddCondition(dfeta_qtsregistration.Fields.StateCode, ConditionOperator.Equal, (int)TaskState.Open);
        queryExpression.Criteria.AddCondition(dfeta_qtsregistration.Fields.dfeta_PersonId, ConditionOperator.In, contactIdsArray.Cast<object>().ToArray());

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        var qtsRegistrationsByContactIds = response.Entities
            .GroupBy(r => r.GetAttributeValue<EntityReference>(dfeta_qtsregistration.Fields.dfeta_PersonId).Id)
            .ToDictionary(group => group.Key, group => group.Select(e => e.ToEntity<dfeta_qtsregistration>()).ToArray());

        return query.ContactIds.ToDictionary(id => id, id => qtsRegistrationsByContactIds.GetValueOrDefault(id, Array.Empty<dfeta_qtsregistration>()));
    }
}
