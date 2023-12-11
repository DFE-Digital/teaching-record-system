using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CloseSanctionHandler : ICrmQueryHandler<CloseSanctionQuery, bool>
{
    public async Task<bool> Execute(CloseSanctionQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_sanction()
            {
                Id = query.SanctionId,
                dfeta_EndDate = query.EndDate.ToDateTimeWithDqtBstFix(isLocalTime: true),
                dfeta_Spent = true
            }
        });

        return true;
    }
}
