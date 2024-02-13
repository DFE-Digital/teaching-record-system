using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetQualificationsByContactIdHandler : ICrmQueryHandler<GetQualificationsByContactIdQuery, dfeta_qualification[]>
{
    public async Task<dfeta_qualification[]> Execute(GetQualificationsByContactIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_qualification.Fields.dfeta_PersonId, ConditionOperator.Equal, query.ContactId);
        filter.AddCondition(dfeta_qualification.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_qualificationState.Active);

        var columns = new List<string>()
        {
            dfeta_qualification.PrimaryIdAttribute,
            dfeta_qualification.Fields.dfeta_Type,
            dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
            dfeta_qualification.Fields.dfeta_MQ_MQEstablishmentId,
            dfeta_qualification.Fields.dfeta_MQStartDate,
            dfeta_qualification.Fields.dfeta_MQ_SpecialismId,
            dfeta_qualification.Fields.dfeta_MQ_Date,
            dfeta_qualification.Fields.dfeta_MQ_Status,
            dfeta_qualification.Fields.StateCode
        };

        // Ensure we have enough to satisfy the sync
        columns.AddRange(TrsDataSyncHelper.GetEntityInfoForModelType(TrsDataSyncHelper.ModelTypes.MandatoryQualification).AttributeNames);

        var queryExpression = new QueryExpression(dfeta_qualification.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(columns.Distinct().ToArray()),
            Criteria = filter
        };

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var result = (RetrieveMultipleResponse)await organizationService.ExecuteAsync(request);
        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_qualification>()).ToArray();
    }
}
