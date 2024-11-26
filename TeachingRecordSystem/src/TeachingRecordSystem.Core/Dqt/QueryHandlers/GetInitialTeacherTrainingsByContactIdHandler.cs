using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetInitialTeacherTrainingsByContactIdHandler : ICrmQueryHandler<GetActiveInitialTeacherTrainingsByContactIdQuery, dfeta_initialteachertraining[]>
{
    public async Task<dfeta_initialteachertraining[]> ExecuteAsync(GetActiveInitialTeacherTrainingsByContactIdQuery queryItt, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_initialteachertraining.Fields.dfeta_PersonId, ConditionOperator.Equal, queryItt.ContactId);
        filter.AddCondition(dfeta_initialteachertraining.Fields.StateCode, ConditionOperator.Equal, dfeta_initialteachertrainingState.Active);

        var query = new QueryExpression(dfeta_initialteachertraining.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(new string[] {
                dfeta_initialteachertraining.PrimaryIdAttribute,
                dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                dfeta_initialteachertraining.Fields.dfeta_Subject3Id,
            }),
            Criteria = filter
        };
        var result = await organizationService.RetrieveMultipleAsync(query);
        var itt = result.Entities.Select(entity => entity.ToEntity<dfeta_initialteachertraining>());
        return itt.ToArray();
    }
}
