using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateInductionPeriodHandler : ICrmQueryHandler<UpdateInductionPeriodQuery, bool>
{
    public async Task<bool> Execute(UpdateInductionPeriodQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.UpdateAsync(new dfeta_inductionperiod()
        {
            Id = query.InductionPeriodID!.Value,
            dfeta_InductionId = query.InductionID!.Value.ToEntityReference(dfeta_induction.EntityLogicalName),
            dfeta_AppropriateBodyId = query.AppropriateBodyID!.Value.ToEntityReference(Account.EntityLogicalName),
            dfeta_StartDate = query.InductionStartDate,
            dfeta_EndDate = query.InductionEndDate,
        });

        return true;
    }
}
