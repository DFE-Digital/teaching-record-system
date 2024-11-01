using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateQualificationHandler : ICrmQueryHandler<CreateQualificationQuery, Guid>
{
    public async Task<Guid> Execute(CreateQualificationQuery query, IOrganizationServiceAsync organizationService)
    {
        return await Task.FromResult(Guid.NewGuid());
    }
}
