using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetInductionByContactIdHandler : ICrmQueryHandler<GetActiveInductionByContactIdQuery, InductionRecord>
{
    public async Task<InductionRecord> Execute(GetActiveInductionByContactIdQuery getInductionQuery, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_induction.Fields.dfeta_PersonId, ConditionOperator.Equal, getInductionQuery.ContactId);
        filter.AddCondition(dfeta_induction.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_inductionState.Active);

        var query = new QueryExpression(dfeta_induction.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(new string[] {
                dfeta_induction.PrimaryIdAttribute,
                dfeta_induction.Fields.dfeta_StartDate,
                dfeta_induction.Fields.dfeta_CompletionDate,
                dfeta_induction.Fields.dfeta_InductionStatus
            }),
            Criteria = filter
        };

        //inductionperiod
        var inductionPeriodLink = query.AddLink(
            dfeta_inductionperiod.EntityLogicalName,
            dfeta_induction.PrimaryIdAttribute,
            dfeta_inductionperiod.Fields.dfeta_InductionId,
            JoinOperator.LeftOuter);
        inductionPeriodLink.Columns = new ColumnSet(new[]
        {
            dfeta_inductionperiod.Fields.dfeta_InductionId,
            dfeta_inductionperiod.Fields.dfeta_StartDate,
            dfeta_inductionperiod.Fields.dfeta_EndDate,
            dfeta_inductionperiod.Fields.dfeta_Numberofterms,
            dfeta_inductionperiod.Fields.dfeta_AppropriateBodyId
        });
        inductionPeriodLink.EntityAlias = dfeta_inductionperiod.EntityLogicalName;

        //account
        var appropriateBodyLink = inductionPeriodLink.AddLink(
            Account.EntityLogicalName,
            dfeta_inductionperiod.Fields.dfeta_AppropriateBodyId,
            Account.PrimaryIdAttribute,
            JoinOperator.LeftOuter);
        appropriateBodyLink.Columns = new ColumnSet(new[]
        {
            Account.PrimaryIdAttribute,
            Account.Fields.Name
        });
        appropriateBodyLink.EntityAlias = $"{dfeta_inductionperiod.EntityLogicalName}.appropriatebody";

        var result = await organizationService.RetrieveMultipleAsync(query);
        var inductionAndPeriods = result.Entities.Select(entity => entity.ToEntity<dfeta_induction>())
            .Select(i => (Induction: i, InductionPeriod: i.Extract<dfeta_inductionperiod>(dfeta_inductionperiod.EntityLogicalName, dfeta_induction.PrimaryIdAttribute)));

        var returnValue = inductionAndPeriods
            .GroupBy(t => t.Induction.Id)
            .Select(g => (g.First().Induction, g.Where(i => i.InductionPeriod != null).Select(i => i.InductionPeriod).OrderBy(p => p.dfeta_StartDate).ToArray()))
            .OrderBy(i => i.Induction.CreatedOn ?? DateTime.MinValue)
            .FirstOrDefault();
        return new InductionRecord(returnValue.Induction, returnValue.Item2);
    }
}
