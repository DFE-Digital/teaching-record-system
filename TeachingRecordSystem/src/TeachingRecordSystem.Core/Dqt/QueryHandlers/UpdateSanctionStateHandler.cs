using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateSanctionStateHandler : ICrmQueryHandler<UpdateSanctionStateQuery, bool>
{
    public async Task<bool> Execute(UpdateSanctionStateQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_sanction()
            {
                Id = query.SanctionId,
                StateCode = query.State
            }
        });

        return true;
    }
}
