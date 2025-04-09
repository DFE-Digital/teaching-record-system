using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateDqtOutboxMessageHandler : ICrmQueryHandler<CreateDqtOutboxMessageQuery, Guid>
{
    public Task<Guid> ExecuteAsync(CreateDqtOutboxMessageQuery query, IOrganizationServiceAsync organizationService)
    {
        var serializer = new MessageSerializer();
        var message = serializer.CreateCrmOutboxMessage(query.Message);

        return organizationService.CreateAsync(message);
    }
}
