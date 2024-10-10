using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class DeleteIntegrationTransactionHandler : ICrmQueryHandler<DeleteIntegrationTransactionQuery, bool>
{
    public async Task<bool> ExecuteAsync(DeleteIntegrationTransactionQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.DeleteAsync(dfeta_integrationtransaction.EntityLogicalName, query.IntegrationTransactionId);
        return true;
    }
}
