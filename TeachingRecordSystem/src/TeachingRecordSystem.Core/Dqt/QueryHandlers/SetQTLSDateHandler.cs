using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class SetQTLSDateHandler : ICrmQueryHandler<SetQTLSDateQuery, bool>
{
    public async Task<bool> Execute(SetQTLSDateQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = query.contactId,
                //dfeta_QTLS_Date = query.qtlsDate.ToDateTimeWithDqtBstFix(isLocalTime: false)
            }
        });

        return true;
    }
}
