using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetActiveInitialTeacherTrainingRecordByContactIdAndSlugIdHandler : ICrmQueryHandler<GetActiveInitialTeacherTrainingRecordByContactIdAndSlugIdQuery, dfeta_initialteachertraining?>
{
    public async Task<dfeta_initialteachertraining?> ExecuteAsync(GetActiveInitialTeacherTrainingRecordByContactIdAndSlugIdQuery queryItt, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_initialteachertraining.Fields.dfeta_PersonId, ConditionOperator.Equal, queryItt.ContactId);
        filter.AddCondition(dfeta_initialteachertraining.Fields.dfeta_SlugId, ConditionOperator.Equal, queryItt.SlugId);
        filter.AddCondition(dfeta_initialteachertraining.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_initialteachertrainingState.Active);

        var query = new QueryExpression(dfeta_initialteachertraining.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(new string[] {
                dfeta_initialteachertraining.Fields.dfeta_PersonId,
                dfeta_initialteachertraining.Fields.dfeta_SlugId,
                dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                dfeta_initialteachertraining.Fields.dfeta_CohortYear,
                dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                dfeta_initialteachertraining.Fields.dfeta_Subject3Id,
                dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                dfeta_initialteachertraining.Fields.dfeta_Result,
                dfeta_initialteachertraining.Fields.dfeta_TraineeID,
                dfeta_initialteachertraining.Fields.dfeta_ITTQualificationId,
                dfeta_initialteachertraining.Fields.dfeta_CountryId,
                dfeta_initialteachertraining.Fields.dfeta_qtsregistration
            }),
            Criteria = filter
        };
        var result = await organizationService.RetrieveMultipleAsync(query);
        return result.Entities.Select(entity => entity.ToEntity<dfeta_initialteachertraining>()).FirstOrDefault();
    }
}
