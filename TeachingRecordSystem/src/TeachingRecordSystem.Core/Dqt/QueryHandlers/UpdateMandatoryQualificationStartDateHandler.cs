using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateMandatoryQualificationStartDateHandler : ICrmQueryHandler<UpdateMandatoryQualificationStartDateQuery, bool>
{
    public async Task<bool> Execute(UpdateMandatoryQualificationStartDateQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = query.QualificationId,
                dfeta_MQStartDate = query.StartDate.ToDateTimeWithDqtBstFix(isLocalTime: true)
            }
        });

        return true;
    }
}
