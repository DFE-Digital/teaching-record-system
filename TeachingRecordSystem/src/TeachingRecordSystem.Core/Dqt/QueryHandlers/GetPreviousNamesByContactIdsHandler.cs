using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetPreviousNamesByContactIdsHandler : ICrmQueryHandler<GetPreviousNamesByContactIdsQuery, IDictionary<Guid, dfeta_previousname[]>>
{
    public async Task<IDictionary<Guid, dfeta_previousname[]>> Execute(GetPreviousNamesByContactIdsQuery query, IOrganizationServiceAsync organizationService)
    {
        var contactIdsArray = query.ContactIds.ToArray();

        if (contactIdsArray.Length == 0)
        {
            return new Dictionary<Guid, dfeta_previousname[]>();
        }

        var queryExpression = new QueryExpression(dfeta_previousname.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                dfeta_previousname.PrimaryIdAttribute,
                dfeta_previousname.Fields.CreatedOn,
                dfeta_previousname.Fields.dfeta_ChangedOn,
                dfeta_previousname.Fields.dfeta_name,
                dfeta_previousname.Fields.dfeta_PersonId,
                dfeta_previousname.Fields.dfeta_Type)
        };

        queryExpression.Criteria.AddCondition(dfeta_previousname.Fields.dfeta_PersonId, ConditionOperator.In, contactIdsArray.Cast<object>().ToArray());
        queryExpression.Criteria.AddCondition(dfeta_previousname.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_documentState.Active);
        queryExpression.Criteria.AddCondition(dfeta_previousname.Fields.dfeta_Type, ConditionOperator.NotEqual, (int)dfeta_NameType.Title);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var result = (RetrieveMultipleResponse)await organizationService.ExecuteAsync(request);

        var previousNamesByContactId = result.EntityCollection.Entities
            .GroupBy(r => r.GetAttributeValue<EntityReference>(dfeta_previousname.Fields.dfeta_PersonId).Id)
            .ToDictionary(
                group => group.Key,
                group => group.Select(e => e.ToEntity<dfeta_previousname>()).ToArray());

        return contactIdsArray.ToDictionary(id => id, id => previousNamesByContactId.GetValueOrDefault(id, Array.Empty<dfeta_previousname>()));
    }
}
