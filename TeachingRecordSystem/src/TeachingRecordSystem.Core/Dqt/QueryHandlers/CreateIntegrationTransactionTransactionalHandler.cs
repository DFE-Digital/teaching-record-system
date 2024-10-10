using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateIntegrationTransactionTransactionalHandler : ICrmQueryHandler<CreateIntegrationTransactionQuery, Guid>
{
    public async Task<Guid> ExecuteAsync(CreateIntegrationTransactionQuery query, IOrganizationServiceAsync organizationService)
    {
        var integrationTransactionId = await organizationService.CreateAsync(new dfeta_integrationtransaction()
        {
            Id = Guid.NewGuid(),
            dfeta_Interface = dfeta_IntegrationInterface.GTCWalesImport,
            dfeta_StartDate = query.StartDate,
            dfeta_Filename = query.FileName
        });
        return integrationTransactionId;
    }
}
