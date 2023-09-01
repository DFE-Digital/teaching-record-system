using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CancelIncidentHandler : ICrmQueryHandler<CancelIncidentQuery, bool>
{
    public async Task<bool> Execute(CancelIncidentQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Incident()
            {
                Id = query.IncidentId,
                StateCode = IncidentState.Canceled,
                StatusCode = Incident_StatusCode.Canceled
            }
        });

        return true;
    }
}
