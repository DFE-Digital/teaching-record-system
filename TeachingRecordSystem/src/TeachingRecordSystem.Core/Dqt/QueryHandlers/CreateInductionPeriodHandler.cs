using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateInductionPeriodHandler : ICrmQueryHandler<CreateInductionPeriodQuery, Guid>
{
    public async Task<Guid> Execute(CreateInductionPeriodQuery query, IOrganizationServiceAsync organizationService)
    {
        var inductionPeriodId = await organizationService.CreateAsync(new dfeta_inductionperiod()
        {
            dfeta_InductionId = query.InductionID!.Value.ToEntityReference(dfeta_induction.EntityLogicalName),
            dfeta_AppropriateBodyId = query.AppropriateBodyID!.Value.ToEntityReference(Account.EntityLogicalName),
            dfeta_StartDate = query.InductionStartDate,
            dfeta_EndDate = query.InductionEndDate,
        });

        return inductionPeriodId;
    }
}
