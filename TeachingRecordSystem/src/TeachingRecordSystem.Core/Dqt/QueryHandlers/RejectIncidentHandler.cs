using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class RejectIncidentHandler : ICrmQueryHandler<RejectIncidentQuery, bool>
{
    public async Task<bool> Execute(RejectIncidentQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new CloseIncidentRequest()
        {
            IncidentResolution = new IncidentResolution()
            {
                IncidentId = new EntityReference(Incident.EntityLogicalName, query.IncidentId),
                Subject = query.RejectionReason,
            },
            Status = new OptionSetValue((int)Incident_StatusCode.Rejected),
        });

        return true;
    }
}
