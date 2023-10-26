using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetEntityMetadataHandler : ICrmQueryHandler<GetEntityMetadataQuery, EntityMetadata>
{
    public async Task<EntityMetadata> Execute(GetEntityMetadataQuery query, IOrganizationServiceAsync organizationService)
    {
        var entityResponse = (RetrieveEntityResponse)await organizationService.ExecuteAsync(new RetrieveEntityRequest()
        {
            LogicalName = query.EntityLogicalName,
            EntityFilters = query.EntityFilters
        });

        return entityResponse.EntityMetadata;
    }
}
