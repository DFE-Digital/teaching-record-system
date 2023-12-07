using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class DeleteQualificationHandler : ICrmQueryHandler<DeleteQualificationQuery, bool>
{
    public async Task<bool> Execute(DeleteQualificationQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = query.QualificationId,
                StateCode = dfeta_qualificationState.Inactive,
                dfeta_TrsDeletedEvent = query.SerializedEvent
            }
        });

        return true;
    }
}
