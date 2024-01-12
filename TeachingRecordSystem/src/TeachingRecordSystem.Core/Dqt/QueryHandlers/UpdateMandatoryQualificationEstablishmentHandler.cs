using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateMandatoryQualificationEstablishmentHandler : ICrmQueryHandler<UpdateMandatoryQualificationEstablishmentQuery, bool>
{
    public async Task<bool> Execute(UpdateMandatoryQualificationEstablishmentQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = query.QualificationId,
                dfeta_MQ_MQEstablishmentId = query.MqEstablishmentId.ToEntityReference(dfeta_mqestablishment.EntityLogicalName),
                dfeta_TRSEvent = EventInfo.Create(query.Event).Serialize()
            }
        });

        return true;
    }
}
