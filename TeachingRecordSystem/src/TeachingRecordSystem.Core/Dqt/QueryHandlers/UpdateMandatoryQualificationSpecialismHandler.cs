using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateMandatoryQualificationSpecialismHandler : ICrmQueryHandler<UpdateMandatoryQualificationSpecialismQuery, bool>
{
    public async Task<bool> Execute(UpdateMandatoryQualificationSpecialismQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = query.QualificationId,
                dfeta_MQ_SpecialismId = query.SpecialismId.ToEntityReference(dfeta_specialism.EntityLogicalName),
                dfeta_TRSEvent = EventInfo.Create(query.Event).Serialize()
            }
        });

        return true;
    }
}
