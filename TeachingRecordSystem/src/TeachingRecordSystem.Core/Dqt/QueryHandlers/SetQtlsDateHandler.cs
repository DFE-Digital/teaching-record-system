using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class SetQtlsDateHandler : ICrmQueryHandler<SetQtlsDateQuery, bool>
{
    public async Task<bool> Execute(SetQtlsDateQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = query.contactId,
                dfeta_qtlsdate = query.qtlsDate.ToDateTimeWithDqtBstFix(isLocalTime: false)
            }
        });

        return true;
    }
}
