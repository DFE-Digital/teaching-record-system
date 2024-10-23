using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class DeleteIntegrationTransactionHandler : ICrmQueryHandler<DeleteIntegrationTransactionQuery, bool>
{
    public async Task<bool> Execute(DeleteIntegrationTransactionQuery query, IOrganizationServiceAsync organizationService)
    {
        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);
        requestBuilder.AddRequest(new DeleteRequest() { Target = new(dfeta_integrationtransaction.EntityLogicalName, query.IntegrationTransactionId) });
        await requestBuilder.Execute();

        return true;
    }
}
