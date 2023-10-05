using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetSanctionDetailsByContactIdHandler : ICrmQueryHandler<GetSanctionDetailsByContactIdQuery, SanctionDetailResult[]>
{
    public async Task<SanctionDetailResult[]> Execute(GetSanctionDetailsByContactIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_sanction.Fields.dfeta_PersonId, ConditionOperator.Equal, query.ContactId);

        var queryExpression = new QueryExpression(dfeta_sanction.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                dfeta_sanction.PrimaryIdAttribute,
                dfeta_sanction.Fields.dfeta_SanctionDetails,
                dfeta_sanction.Fields.dfeta_PersonId,
                dfeta_sanction.Fields.dfeta_StartDate,
                dfeta_sanction.Fields.dfeta_EndDate,
                dfeta_sanction.Fields.dfeta_Spent,
                dfeta_sanction.Fields.dfeta_DetailsLink,
                dfeta_sanction.Fields.StateCode),
            Criteria = filter
        };

        var sanctionCodeLink = queryExpression.AddLink(
            dfeta_sanctioncode.EntityLogicalName,
            dfeta_sanction.Fields.dfeta_SanctionCodeId,
            dfeta_sanctioncode.PrimaryIdAttribute,
            JoinOperator.Inner);

        sanctionCodeLink.Columns = new ColumnSet(dfeta_sanctioncode.PrimaryIdAttribute, dfeta_sanctioncode.Fields.dfeta_name);
        sanctionCodeLink.EntityAlias = typeof(dfeta_sanctioncode).Name;

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var result = (RetrieveMultipleResponse)await organizationService.ExecuteAsync(request);
        return result.EntityCollection.Entities.Select(entity => new SanctionDetailResult(entity.ToEntity<dfeta_sanction>(), entity.Extract<dfeta_sanctioncode>().dfeta_name)).ToArray();
    }
}
