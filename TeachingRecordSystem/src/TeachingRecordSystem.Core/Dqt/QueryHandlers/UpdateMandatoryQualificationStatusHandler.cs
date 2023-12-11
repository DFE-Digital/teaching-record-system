using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateMandatoryQualificationStatusHandler : ICrmQueryHandler<UpdateMandatoryQualificationStatusQuery, bool>
{
    public async Task<bool> Execute(UpdateMandatoryQualificationStatusQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = query.QualificationId,
                dfeta_MQ_Status = query.MqStatus,
                dfeta_MQ_Date = query.EndDate.ToDateTimeWithDqtBstFix(isLocalTime: true)
            }
        });

        return true;
    }
}
