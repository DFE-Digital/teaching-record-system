using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateIntegrationTransactionHandler : ICrmQueryHandler<CreateIntegrationTransactionQuery, Guid>
{
    public async Task<Guid> Execute(CreateIntegrationTransactionQuery query, IOrganizationServiceAsync organizationService)
    {
        var id = Guid.NewGuid();
        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);

        var integrationTransaction = new dfeta_integrationtransaction()
        {
            Id = id,
            dfeta_Interface = dfeta_IntegrationInterface.GTCWalesImport,
            dfeta_StartDate = query.StartDate
        };
        requestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = integrationTransaction });
        await requestBuilder.Execute();

        return id;
    }
}
