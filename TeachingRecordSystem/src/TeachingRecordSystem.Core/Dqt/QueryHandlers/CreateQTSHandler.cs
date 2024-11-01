using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateQTSHandler : ICrmQueryHandler<CreateQTSQuery, Guid>
{
    public async Task<Guid> Execute(CreateQTSQuery query, IOrganizationServiceAsync organizationService)
    {
        var qtsRegistrationId = await organizationService.CreateAsync(new dfeta_qtsregistration()
        {
            dfeta_PersonId = query.PersonId!.Value.ToEntityReference(Contact.EntityLogicalName),
            dfeta_TeacherStatusId = query.TeacherStatusId!.Value.ToEntityReference(dfeta_teacherstatus.EntityLogicalName),
            dfeta_QTSDate = query.QTSDate
        });

        return qtsRegistrationId;
    }
}
