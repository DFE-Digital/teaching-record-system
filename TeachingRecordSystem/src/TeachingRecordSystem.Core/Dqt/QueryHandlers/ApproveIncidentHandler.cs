using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class ApproveIncidentHandler : ICrmQueryHandler<ApproveIncidentQuery, bool>
{
    public async Task<bool> Execute(ApproveIncidentQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new CloseIncidentRequest()
        {
            IncidentResolution = new IncidentResolution()
            {
                IncidentId = new EntityReference(Incident.EntityLogicalName, query.IncidentId),
                Subject = "Approved",
            },
            Status = new OptionSetValue((int)Incident_StatusCode.Approved),
        });

        return true;
    }
}
